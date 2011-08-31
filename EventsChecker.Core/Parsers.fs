namespace EventsChecker.Core

open System
open System.IO
open NLog

module SOReputationParser =
    let parse (str:string) =
        match str.Split('|') with
        | [|name; num|] -> new SOReputationChecker(name.Trim(), num.Trim() |> int) :> IChecker
        | _ -> failwith (sprintf "Unable to parse SOReputationChecker from %s" str)
        
module FlashBlogRepliesCountParser =
    let parse (str:string) =
        new FlashBlogRepliesCountChecker(str.Trim()) :> IChecker
        
module SOQuestionsCheckerParser = 
    let parse (str:string) =
        match str.Split('|') with
        | [|name; url|] -> new SOQuestionsChecker(name.Trim(), url.Trim()) :> IChecker
        | _ -> failwith (sprintf "Unable to parse SOQuestionsChecker from %s" str)
        
module SOAnswersCheckerParser = 
    let parse (str:string) =
        match str.Split('|') with
        | [|name; url|] -> new SOAnswersChecker(name.Trim(), url.Trim()) :> IChecker
        | _ -> failwith (sprintf "Unable to parse SOAnswersChecker from %s" str)
        
module SOCommentsCheckerParser = 
    let parse (str:string) =
        match str.Split('|') with
        | [|name; url|] -> new SOCommentsChecker(name.Trim(), url.Trim()) :> IChecker
        | _ -> failwith (sprintf "Unable to parse SOCommentsChecker from %s" str)
        
module CruiseControlCheckerParser = 
    let parse (str:string) =
        match str.Split('|') with
        | [|name; url|] -> new CruiseControlChecker(name.Trim(), url.Trim()) :> IChecker
        | _ -> failwith (sprintf "Unable to parse CruiseControlChecker from %s" str)

module DirectoryContentCheckerParser = 
    let (|DirectoryCheckerParams|_|) (str: string) =
       match str.Split('|') with
        | [|name; directory; recursiv|] -> 
            match bool.TryParse recursiv with
            | (true, value) -> Some(name, directory, value)
            | _ -> None
        | _ -> None
    let parse (str:string) =
        match str with
        | DirectoryCheckerParams (name, dir, recursiv) -> new DirectoryContentChecker(name.Trim(), dir.Trim(), recursiv) :> IChecker
        | _ -> failwith (sprintf "Unable to parse DirectoryContentChecker from %s" str)
        
module CheckersParsers =
    let logger = LogManager.GetLogger("CheckersParsers")
    let parseChecker (str:string) =
        let spaceAt = str.IndexOf(' ')
        let parserData = str.Substring(spaceAt+1)
        let name = str.Substring(0, spaceAt)
        match name.Trim() with 
        | "SOReputationChecker" -> SOReputationParser.parse parserData
        | "FlashBlogRepliesCountChecker" -> FlashBlogRepliesCountParser.parse parserData
        | "SOQuestionsChecker" -> SOQuestionsCheckerParser.parse parserData
        | "SOAnswersChecker" -> SOAnswersCheckerParser.parse parserData
        | "SOCommentsChecker" -> SOCommentsCheckerParser.parse parserData
        | "CruiseControlChecker" -> CruiseControlCheckerParser.parse parserData
        | "DirectoryContentChecker" -> DirectoryContentCheckerParser.parse parserData
        | _ -> 
            logger.Error(sprintf "unknown checker type: %s" name)
            failwith (sprintf "unknown checker type: %s" name)

module DefinitionParser =
    let logger = LogManager.GetLogger("CheckersParsers")

    let private parseLine (line:string) =
        // parses one line of definition
        let parseLine_() =
            let lineParts = line.Split(' ')
            if lineParts.Length < 4 then 
                failwith (sprintf "Unable to parse line %s, not enough arguments. Length: %d" line lineParts.Length)
            { Interval = Double.Parse(lineParts.[0].Trim())
              Enabled = if lineParts.[1].Trim() = "1" then true else false
              NotifierTypes = lineParts.[2]
              Checker = CheckersParsers.parseChecker (line.Substring(line.IndexOf(lineParts.[3])))
            }
        try
            if String.IsNullOrEmpty(line) || line.StartsWith("#") then
                None
            else
                Some(parseLine_())
        with ex ->
            logger.Error((sprintf "Error when parsing line %s." line), ex)
            printfn "Error when parsing line %s. %A" line ex
            None 

    let parse path =
        if not (File.Exists(path)) then
            failwith (sprintf "Definition file %s doesn't exist" path)
        seq {
            for line in File.ReadAllLines(path) do
                match parseLine line with
                | Some(c) -> yield c
                | None -> ()
        }