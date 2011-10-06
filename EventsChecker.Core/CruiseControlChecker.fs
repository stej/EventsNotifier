namespace EventsChecker.Core

open System
open System.Xml
open System.Text.RegularExpressions

type CruiseControlChecker(project, url) =
    inherit SimpleValueCheckerBase<string>("ccnet-"+project) //
    
    override this.HasValueChanged oldv newv =
        oldv <> newv
        
    override this.ConvertValueToStore value =
        value
        
    override this.GetNewestValue() =
        Downloader.downloadPage url
          |> HtmlDownloader.html2Xml
          |> Xml.xpathNodes "//table[@class='RecentBuildsPanel']/tr//a[contains(@href, 'ViewBuildReport.aspx')]"
          |> Seq.cast<XmlElement>
          |> Seq.nth 0
          |> fun node -> 
              //let text = node.InnerText.Split(' ')
              //DateTime.Parse($text.[0] + ' ' + $text.[1]), Regex.Replace($text.[2], "\)|\(", "")
              node.InnerText
        
    override this.NotifyChange() =
        { CheckerHeader = sprintf "Project %s build finished." project 
          Details       = [sprintf "Status: %s" this.ChangedValue.Value] }

    override this.ToString() =
        sprintf "CruiseControlChecker - %s (%s)" project url

    override this.GetName() =
        project