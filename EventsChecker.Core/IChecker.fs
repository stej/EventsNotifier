namespace EventsChecker.Core

open System

type IChecker =
    abstract CheckChange : unit -> bool
    abstract ReportChangedValue : unit -> string list
    abstract GetLastCheckDate : unit -> DateTime
    abstract Error : IEvent<string * Exception>
    abstract StartedChecking : IEvent<IChecker>
    abstract FinishedChecking : IEvent<IChecker>

type CheckerDefinition = {
    Interval : float
    Enabled : bool
    Checker : IChecker
    NotifierTypes : string
}