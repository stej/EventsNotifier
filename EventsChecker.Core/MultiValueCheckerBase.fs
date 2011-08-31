namespace EventsChecker.Core

(*open System
open System.Xml
open System.Text.RegularExpressions

type MultiValueCheckerBase(name, url : string) as this =

    let error = new Event<string * Exception>()
    let startedChecking = new Event<IChecker>()
    let finishedChecking = new Event<IChecker>()


    interface IChecker with
        member this.Error =  error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish
*)