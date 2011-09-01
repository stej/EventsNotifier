namespace EventsChecker.Core

open System
open System.IO

type SimpleValueStorer(caller: obj, someId: string) =
    let path = 
        let baseDir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data")
        Path.Combine(baseDir, sprintf "svs-%s.txt" someId)

    member x.GetValue() =
        if File.Exists path then
            match File.ReadAllLines(path) with
            | [|date; value|] -> DateTime.Parse(date), value
            | _ -> failwith (sprintf "File %s doesn't contain date and value" path)
        else
            System.DateTime.MinValue, ""

    member x.SetValue(v) =
        File.WriteAllLines(path, [|DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); v|])

    member x.GetDate() =
        if File.Exists path then
            match File.ReadAllLines(path) with
            | [|date; _|] -> DateTime.Parse(date)
            | _ -> failwith (sprintf "File %s doesn't contain date" path)
        else
            System.DateTime.MinValue
    
    member x.UpdateDate() =
        if File.Exists path then
            match File.ReadAllLines(path) with
            | [|date; value|] -> x.SetValue(value)
            | _ -> failwith (sprintf "File %s doesn't contain date" path)
        else
            x.SetValue("")