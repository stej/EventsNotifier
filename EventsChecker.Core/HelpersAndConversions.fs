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

module Xml = 
    let xpathValue path (xml:XmlNode) =
        xml.SelectSingleNode(path).InnerText

    let xpathNodes path (xml:XmlNode) =
        xml.SelectNodes(path)