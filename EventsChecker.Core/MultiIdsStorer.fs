namespace EventsChecker.Core

open System
open System.IO

/// Stores ids one by line
/// First line is for date of last check, the rest are ids
type MultiIdsStorer(caller: obj, someId: string) =
    let path = 
        let baseDir = System.AppDomain.CurrentDomain.BaseDirectory
        Path.Combine(baseDir, sprintf "mis-%s.txt" someId)
        
    let getDate() = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    let getDateMin() = DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss")
    
    let ensureFileExists() =
        if not (File.Exists path) then
            File.WriteAllLines(path, [|yield getDateMin()|])
        
    /// Takes tuples (id * full object) and returns only full objects whose ids
    /// are not contained in  the file
    member x.FilterNewValues<'a> (values: (string * 'a) seq) =
        ensureFileExists()
        let set = new Set<string>(File.ReadAllLines(path) |> Seq.skip 1) // skip date
        // filter values (tuple) where fst is not contained in file
        values |> Seq.filter (fun (id, o) -> not (set.Contains(id)))
        
    /// Takes only ids (object itself are ids, don't have more members) ids that
    /// are not contained in  the file
    member x.FilterNewValues (values: string seq) =
        ensureFileExists()
        let set = new Set<string>(File.ReadAllLines(path) |> Seq.skip 1) // skip date
        // filter values (tuple) where fst is not contained in file
        values |> Seq.filter (fun id -> not (set.Contains(id)))
            
    /// Takes array of ids as strings as values
    member x.AddNewValues(values: string seq) =
        ensureFileExists()
        match File.ReadAllLines(path) |> List.ofArray with
        | date::ids -> File.WriteAllLines(path, getDate()::ids)
        | _ -> failwith "File %s doesn't contain proper date and ids"
        File.AppendAllLines(path, [|for v in values -> v|])
        
    member x.GetDate() =
        ensureFileExists()
        match File.ReadAllLines(path) |> List.ofArray with
        | date::rest -> DateTime.Parse(date)
        | _ -> failwith (sprintf "File %s doesn't contain date" path)

      
    member x.UpdateDate() =
        ensureFileExists()
        let lines = File.ReadAllLines(path)
        let toWrite = [yield getDate()
                       yield! (lines |> Seq.skip 1)]
        File.WriteAllLines(path, toWrite)