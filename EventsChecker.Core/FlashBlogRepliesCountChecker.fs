namespace EventsChecker.Core

open System
open System.Text.RegularExpressions

type FlashBlogRepliesCountChecker(url : string) =
    inherit SimpleValueCheckerBase<int>("flashblog-" +
        Regex.Replace(url, ".*/([^/]+)\.\w+$", "$1").Replace("-",""))
    
    override this.HasValueChanged oldv newv =
        oldv <> newv.ToString()
        
    override this.ConvertValueToStore value =
        value |> string
        
    override this.GetNewestValue() =
        Downloader.downloadPage url 
            |> HtmlDownloader.html2Xml
            |> Xml.xpathValue "//a[@href='#comments']"
            |> Conversions.IntOrDefault
            
    override this.NotifyChange() =
        [sprintf "Somebody commented blog %s. Comments: %d" url this.ChangedValue.Value]

    override this.ToString() =
        sprintf "FlashBlogRepliesCountChecker - %s" url

    override this.GetName() =
        url