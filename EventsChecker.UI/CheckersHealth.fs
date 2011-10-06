namespace EventsChecker.UI

open System.Collections.Generic
open EventsChecker.Core
open NLog

type NewlyFoundRepliesMessages =
| RegisterSuccess of CheckerInfo
| RegisterFailure of CheckerInfo
| GetNextInterval of CheckerInfo * AsyncReplyChannel<float>

type CheckersHealth() = 
    let logger = LogManager.GetLogger("CheckersHealth") 
    let mbox = 
        MailboxProcessor.Start(fun mbox ->
            let rec loop (failures : Dictionary<CheckerDefinition, int>) = async {
                let! msg = mbox.Receive()
                
                match msg with
                | RegisterSuccess(info) ->
                    failures.[info.Definition] <- 0
                    return! loop failures

                | RegisterFailure(info) ->
                    failures.[info.Definition] <- failures.[info.Definition] + 1
                    return! loop failures

                | GetNextInterval(info, chnl) ->
                    let definition = info.Definition
                    let interval = 
                        match failures.TryGetValue(definition) with
                        | true, value when value = 0 -> definition.Interval
                        | true, value when value <= 3 -> definition.Interval * (float value)
                        | true, value when value > 3 -> CheckersHealth.InfiniteInterval
                        | _ ->
                            logger.Debug("Failures for {0} not found", info.Checker)
                            failures.[definition] <- 0
                            definition.Interval
                    logger.Debug("Next interval for {0} is {1}", definition.Checker, interval)
                    chnl.Reply(interval)
                    return! loop failures
            }
            loop (new Dictionary<CheckerDefinition, int>())
        )
    do
        mbox.Error.Add(fun exn -> logger.ErrorException("Error in CheckersHealth", exn)
                                  System.Windows.Forms.MessageBox.Show(exn.Message, "Error...") |> ignore)
    member x.RegisterSuccess(info) = mbox.Post(RegisterSuccess(info))
    member x.RegisterFailure(info) = mbox.Post(RegisterFailure(info))
    member x.AsyncGetNextInterval(info) = mbox.PostAndAsyncReply(fun reply -> GetNextInterval(info, reply))
    member x.GetNextInterval(info) = mbox.PostAndReply(fun reply -> GetNextInterval(info, reply))

    static member InfiniteInterval = -1.
    static member IsInfiniteInterval(interval) = interval < 0.1  // don't mess with abs..