
namespace EventsChecker.Core

open System
open System.Xml
open System.IO
open NLog


type SkillsMatterWebChecker(url: string, textAnchor : string, urlHref: string) as this =
    let storer = new MultiIdsStorer(this, "SkillsMatter-"+url.Replace("\\", "").Replace(":", "").Replace("/",""))

    let error = new Event<string * Exception>()
    let startedChecking = new Event<IChecker>()
    let finishedChecking = new Event<IChecker>()

    let logger = NLog.LogManager.GetLogger(this.GetType().Name)

    let trimWhiteSpacace (str : string) =
        str.Trim([|'\t';'\n';'\r'; ' '|])

    let mutable changedValues = None
    member this.ChangedValues
        with get() = changedValues
    
    interface IChecker with
        member this.CheckChange() = 
            try 
                logger.Info("Checking: '{0}'", url)
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
                logger.Info("Finished: '{0}'", url)
                finishedChecking.Trigger(this)
                res
            with ex ->
                logger.ErrorException((sprintf "Error when checking %s." url), ex)
                storer.UpdateDate()
                error.Trigger((sprintf "Directory %s doesn't exist." url), ex)
                false

        member this.ReportChangedValue() =
            { CheckerHeader = "New courses at "+ url
              Details       = this.ChangedValues.Value |> Array.toList }
        member this.GetLastCheckDate() =
            storer.GetDate()

        member this.Error = error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish

    member this.GetNewestValues() =
        let getTdLinkText (td: XmlNode) = 
            let getTextFromElement (node : XmlNode) =
                match node.NodeType with
                | XmlNodeType.Text -> trimWhiteSpacace node.Value
                | XmlNodeType.Element -> trimWhiteSpacace node.InnerText
                | _ -> ""
            let text = System.String.Join(" ", [|for n in td.ChildNodes -> getTextFromElement n|])
            text
        Downloader.downloadPage url 
            |> HtmlDownloader.html2Xml
            |> Xml.xpathNodes (sprintf "//div[contains(text(), '%s')]/following-sibling::table//td[a[contains(@href, '%s')]]" textAnchor urlHref)
            |> Seq.cast<XmlNode>
            |> Seq.map (fun node -> (getTdLinkText node))
            |> Seq.filter (fun text -> (trimWhiteSpacace text) <> "")
            
    override this.ToString() =
        sprintf "SkillsMatterWebChecker - %s" url