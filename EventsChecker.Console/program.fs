module EventsChecker.Console.Program

open EventsChecker.Core
open System

let errors = new System.Collections.Generic.List<string * Exception>()

let domap fce item =
    fce item
    item
let registerCheckerError def =
    Event.add (fun e -> errors.Add(e)) def.Checker.Error
DefinitionParser.parse "checkers.txt" 
    |> Seq.toList
    //|> List.map (domap (fun d -> printfn "%A %A %A" d.Interval d.Enabled d.Checker))
    |> List.map (domap registerCheckerError)
    |> List.filter (fun d -> d.Checker.GetLastCheckDate().AddMinutes(d.Interval) < DateTime.Now &&
                             d.Enabled)
    |> List.map (fun d -> printf "."; (d, d.Checker.CheckChange()))
    |> List.filter snd
    |> domap (fun _ -> printfn "")
    |> List.iter (fun (d, res) -> for v in d.Checker.ReportChangedValue() do printf "%s\n" v)
    
if errors.Count > 0 then
    printfn "\nErrors:--------"
    for (s, e) in errors do
        printfn "%s" s
        printfn "%A" e