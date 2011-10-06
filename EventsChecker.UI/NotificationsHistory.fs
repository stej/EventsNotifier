namespace EventsChecker.UI

open System
open System.Windows.Forms
open EventsChecker.Core
open NLog

type CheckerNotifications = {
    Header : string
    Details : string list
}

type HistoryMessage =  
    | Clear
    | Add of CheckerChangedValue
    | Get of AsyncReplyChannel<CheckerNotifications list>

type NotificationHistory() =  
    let logger = LogManager.GetLogger("NotificationsHistory") 
    let mbox = 
        MailboxProcessor.Start(fun mbox ->
            let rec loop (notifications:Map<string, string list>) = async {
                let! msg = mbox.Receive()
                logger.Debug("Preview state message: {0}", msg.ToString())
                match msg with
                | Add(toAdd) ->
                    match notifications.TryFind(toAdd.CheckerHeader) with
                    | None    -> let newNotifications = notifications |> Map.add toAdd.CheckerHeader toAdd.Details
                                 return! loop newNotifications
                    | Some(v) -> let newNotifications = notifications |> Map.add toAdd.CheckerHeader (v@toAdd.Details)
                                 return! loop newNotifications
                | Clear ->
                    return! loop (Map.empty)
                | Get(chnl) ->
                    let ret = notifications |> Map.toList 
                                            |> List.map (fun (k,v) -> { Header = k; Details = v })
                    chnl.Reply(ret)
                    return! loop notifications
            }
            loop (Map.empty)
        )
    do
        mbox.Error.Add(fun exn -> logger.ErrorException("Error in History", exn)
                                  System.Windows.Forms.MessageBox.Show(exn.Message, "Error...") |> ignore)

    member x.Add(n) = mbox.Post(Add(n))
    member x.Clear() = mbox.Post(Clear)
    member x.AsyncGet() = mbox.PostAndAsyncReply(fun reply -> Get(reply))

    static member InfiniteInterval = -1.
    static member IsInfiniteInterval(interval) = interval < 0.1  // don't mess with abs..