namespace EventsChecker.Core

open System
open System.IO
open NLog

type DirectoryContentChecker(name : string, directory : string, recursiv) as this =
    let storer = new MultiIdsStorer(this, "directory-"+directory.Replace("\\", "").Replace(":", ""))

    let error = new Event<string * Exception>()
    let startedChecking = new Event<IChecker>()
    let finishedChecking = new Event<IChecker>()

    let logger = NLog.LogManager.GetLogger(this.GetType().Name)

    let mutable changedValues = None
    member this.ChangedValues
        with get() = changedValues
    
    interface IChecker with
        member this.CheckChange() = 
            try 
                logger.Info("Checking: '{0}'", name)
                startedChecking.Trigger(this)
                let newValues = 
                    this.GetNewestValues() 
                    |> storer.FilterNewValues
                    |> Array.ofSeq
                
                let res = 
                    if newValues.Length > 0 then
                        newValues |> storer.AddNewValues
                        changedValues <- newValues |> Some
                        true
                    else
                        storer.UpdateDate()
                        false
                logger.Info("Finished: '{0}'", name)
                finishedChecking.Trigger(this)
                res
            with ex ->
                logger.ErrorException((sprintf "Error when checking dir %s." directory), ex)
                storer.UpdateDate()
                error.Trigger((sprintf "Directory %s doesn't exist." directory), ex)
                false

        member this.ReportChangedValue() =
            [sprintf "New items added to %s" directory] @
            [for v in this.ChangedValues.Value -> "  " + v]
        member this.GetLastCheckDate() =
            storer.GetDate()

        member this.Error = error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish

    member this.GetNewestValues() =
        let rec getDirContent directory =
            seq { yield! System.IO.Directory.GetFiles(directory)
                  for d in Directory.GetDirectories(directory) do
                    yield d
                    if recursiv then
                        yield! getDirContent d
            }
        if Directory.Exists(directory) then
            getDirContent directory |> Seq.toArray
        else
            failwith "Unable to get directory content"
            
    override this.ToString() =
        sprintf "DirectoryContentChecker - %s (%s)" name directory