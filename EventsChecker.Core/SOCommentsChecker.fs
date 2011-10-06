namespace EventsChecker.Core

open System
open System.Xml
open System.Text.RegularExpressions

open NLog

type SOCommentsChecker(name, url : string) as this =
    let storer = new MultiIdsStorer(this, "socomm"+Regex.Replace(name, ".*/([^/]+)$", "$1").Replace("%",""))

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
                logger.ErrorException((sprintf "Error when getting comments for %s" name), ex)
                storer.UpdateDate()
                error.Trigger("Error when getting comments", ex)
                false

        member this.ReportChangedValue() =
            { CheckerHeader = sprintf "Comment(s) added to %s (%s)" name url
              Details       = [sprintf "Count: %d" (this.ChangedValues.Value |> Seq.length)] }
        member this.GetLastCheckDate() =
            storer.GetDate()

        member this.Error = error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish

    member this.GetNewestValues() =
        Downloader.downloadPage url 
            |> HtmlDownloader.html2Xml
            |> Xml.xpathNodes "//div[@class='comments']//tr[@class='comment' and contains(@id, 'comment-')]/@id"
            |> Seq.cast<XmlNode>
            |> Seq.map (fun node -> node.Value)
            
    override this.ToString() =
        sprintf "SOCommentsChecker - %s (%s)" name url