namespace EventsChecker.Core

open System
open System.Xml
open System.Text.RegularExpressions
open NLog

type SOAnswersChecker(name, url : string) as this =
    let storer = new MultiIdsStorer(this, "soansw"+Regex.Replace(name, ".*/([^/]+)$", "$1").Replace("%",""))

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
                logger.ErrorException((sprintf "Error when getting answers for %s" name), ex)
                storer.UpdateDate()
                error.Trigger("Error when getting answers", ex)
                false

        member this.ReportChangedValue() =
            [sprintf "Answer(s) added to %s (%s)" name url] @
            [for v in this.ChangedValues.Value -> sprintf " %s" v] @
            [sprintf " Count: %d" (Seq.length this.ChangedValues.Value)]
        member this.GetLastCheckDate() =
            storer.GetDate()

        member this.Error = error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish

    member this.GetNewestValues() =
        Downloader.downloadPage url 
            |> HtmlDownloader.html2Xml
            |> Xml.xpathNodes "//div[@id='answers']/div[@class='answer']//a[contains(@id, 'link-post-')]/@href"
            |> Seq.cast<XmlNode>
            |> Seq.map (fun node -> node.Value)
            
    override this.ToString() =
        sprintf "SOAnswersChecker - %s (%s)" name url