namespace EventsChecker.Core

open System
open System.Xml
open System.Text.RegularExpressions
open NLog

type SOQuestion = {
    Id : int64
    Url : string
    Title : string
}
type SOQuestionsChecker(name, url : string) as this =
    let storer = new MultiIdsStorer(this, "soq"+ Regex.Replace(url, ".*/([^/]+)$", "$1").Replace("%",""))

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
                    |> Seq.map (fun v -> v.Id.ToString(), v)
                    |> storer.FilterNewValues
                    |> Array.ofSeq
                
                let res = 
                    if newValues.Length > 0 then
                        newValues |> Seq.map fst |> storer.AddNewValues
                        changedValues <- newValues |> Seq.map snd |> Some
                        true
                    else
                        storer.UpdateDate()
                        false

                logger.Info("Finished: '{0}'", name)
                finishedChecking.Trigger(this)
                res
            with ex ->
                logger.Error((sprintf "Error when getting questions for %s" name), ex)
                storer.UpdateDate()
                error.Trigger("Error when getting questions", ex)
                false
                
        member this.ReportChangedValue() =
            [sprintf "New questions for '%s'" name] @
            [for v in this.ChangedValues.Value -> sprintf " %d - %s" v.Id v.Title]
        member this.GetLastCheckDate() =
            storer.GetDate()

        member this.Error =  error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish
            
    member this.ConvertValuesToIds values =
        [| for v in values -> v.Id |> string |]
        
    member this.GetNewestValues() =
        Downloader.downloadPage url 
            |> HtmlDownloader.html2Xml
            |> Xml.xpathNodes "//div[@class='question-summary']/div[@class='summary']"
            |> Seq.cast<XmlNode>
            |> Seq.map (fun node -> 
                { Title = Xml.xpathValue "h3/a/text()" node
                  Url = Xml.xpathValue "h3/a/@href" node
                  Id = Regex.Replace(Xml.xpathValue "h3/a/@href" node, "/questions/(?<id>[^/]+)/.*", "${id}") |> int64
                }
              )

    override this.ToString() =
        sprintf "SOQuestionsChecker - %s (%s)" name url