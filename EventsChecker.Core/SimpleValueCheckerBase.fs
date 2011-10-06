namespace EventsChecker.Core

open System
open NLog

[<AbstractClass>]
type SimpleValueCheckerBase<'SimpleValueType>(storerPartId : string) as this =
    let storer = new SimpleValueStorer(this, storerPartId)

    let error = new Event<string * Exception>()
    let startedChecking = new Event<IChecker>()
    let finishedChecking = new Event<IChecker>()
 
    let logger = NLog.LogManager.GetLogger(this.GetType().Name)

    let mutable changedValue = None
    member this.ChangedValue
        with get() = changedValue
    
    interface IChecker with
        member this.CheckChange() = 
            try
                logger.Info("Checking: '{0}'", this.GetName())
                startedChecking.Trigger(this)
                let currVal = this.GetNewestValue()
                let date, stored = storer.GetValue()
                let res = 
                    if this.HasValueChanged stored currVal then
                        this.ConvertValueToStore currVal |> storer.SetValue
                        changedValue <- Some(currVal)
                        true
                    else
                        storer.UpdateDate()
                        false

                logger.Info("Finished: '{0}'", this.GetName())
                finishedChecking.Trigger(this)
                res
            with ex ->
                logger.ErrorException((sprintf "Error when checking new state: %s" (this.GetName())) , ex)
                storer.UpdateDate()
                error.Trigger("Error when checking new state", ex)
                false

        member this.ReportChangedValue() =
            this.NotifyChange()
        member this.GetLastCheckDate() =
            storer.GetDate()

        member this.Error = error.Publish
        member this.StartedChecking = startedChecking.Publish
        member this.FinishedChecking = finishedChecking.Publish
            
    abstract ConvertValueToStore : 'SimpleValueType -> string
    abstract GetNewestValue : unit -> 'SimpleValueType
    abstract HasValueChanged : string -> 'SimpleValueType -> bool
    abstract NotifyChange : unit -> CheckerChangedValue
    // return name that identifies the checker among other checkers of the same type
    abstract GetName : unit -> string