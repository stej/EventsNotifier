namespace EventsChecker.Core

open System
open System.Net
open System.Xml

module Conversions =
    let Int64OrDefault (value:string) =
        match Int64.TryParse(value) with
            | (true, i) -> i
            | _ -> -1L
    let IntOrDefault (value:string) =
        match Int32.TryParse(value) with
            | (true, i) -> i
            | _ -> -1

module Downloader =
    let downloadPage (url : string) =
        let c = new WebClient()
        ["Accept", "text/html, application/xhtml+xml, */*'"
         "Accept-Language", "cs-CZ"
         "User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)"
         "Accept-Encoding", "identity'"
         ] |> List.iter (fun (h,v) -> c.Headers.Add(h, v))
        c.DownloadString(url)

module JsonDownloader = 
    let downloadJson (url : string) =
        let deserializer = new System.Web.Script.Serialization.JavaScriptSerializer()
        let jsonFlair = (Downloader.downloadPage url 
                         |> deserializer.DeserializeObject) :?> System.Collections.Generic.Dictionary<string, obj>
        jsonFlair
    let parseJsonInt (num : obj) = 
        Int32.Parse(num :?> string, System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture)
        
module HtmlDownloader =        
    let html2Xml (html : string) =
        let h = html.Replace(" xmlns=\"http://www.w3.org/1999/xhtml\"", "")
        use sr = new System.IO.StringReader(h)
        let xml = 
            use sgml = new Sgml.SgmlReader(
                          DocType = "HTML",
                          WhitespaceHandling = System.Xml.WhitespaceHandling.All,
                          CaseFolding = Sgml.CaseFolding.ToLower,
                          InputStream = sr)
            let x = new System.Xml.XmlDocument(
                      PreserveWhitespace = true,
                      XmlResolver = null)
            x.Load(sgml)
            x
        //use reader = new XmlNodeReader(xml)
        //reader.MoveToContent() |> ignore
        //XDocument.Load(reader)
        xml
        
module Xml = 
    (*
    let xattr s (el:XElement) =
        el.Attribute(XName.Get(s)).Value
    let xelem s (el:XContainer) =
        el.Element(XName.Get(s))
    let xvalue (el:XElement) =
        el.Value
    let xelems s (el:XContainer) =
        el.Elements(XName.Get(s))
      
    let xpath path (el:XContainer) =
        let res = Seq.fold (fun xn s -> xn |> xelem s :> XContainer) el path
        res :?> XElement
    *)
    let xpathValue path (xml:XmlNode) =
        xml.SelectSingleNode(path).InnerText

    let xpathNodes path (xml:XmlNode) =
        xml.SelectNodes(path)