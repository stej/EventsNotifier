module EventsChecker.UI.Form0

(*
open System
open System.Drawing
open System.Threading
open EventsChecker.Core
open System.Windows.Forms
open EventsChecker.UI

let errors = new System.Collections.Generic.List<string * Exception>()

let domap fce item =
    fce item
    item
let registerCheckerError (def:CheckerDefinition) =
    Event.add (fun e -> errors.Add(e)) def.Checker.Error

let readDefinition() =
    DefinitionParser.parse "checkers.txt" 
    |> Seq.toList
    |> List.map (domap registerCheckerError)

let checkersDefinition = readDefinition()
let initFormContent (lv:ListView) =
     checkersDefinition 
     |> Seq.map (fun definition -> { Checker = definition.Checker
                                     Control = Controls.addChecker lv definition
                                     Notifier = NotifierParsers.parseNotifier definition.NotifierType
                                     Definition = definition})

let form = new System.Windows.Forms.Form(Text = "test")
let syncContext = System.Threading.SynchronizationContext.Current
let listView, errorsTb, updateBtn = Controls.fillForm form

listView.ListViewItemSorter <- 
    { new System.Collections.IComparer with 
        member this.Compare(x, y) = 
            let item1, item2 = x:?>ListViewItem, y:?>ListViewItem
            DateTime.Compare(Controls.getNextUpdateDate item1, Controls.getNextUpdateDate item2)
    }

let registerEvents info =
    let updateCtlWorking ctl =
        syncContext.Post(new SendOrPostCallback(fun _ -> Controls.setRowState Controls.Working ctl), ())
    let updateCtlWaiting ctl =
        syncContext.Post(new SendOrPostCallback(fun _ -> Controls.setRowState Controls.Waiting ctl), ())
    let updateCtlError ctl =
        syncContext.Post(new SendOrPostCallback(fun _ -> Controls.setRowState Controls.Error ctl), ())

    info.Checker.StartedChecking  |> Event.add (fun _ -> updateCtlWorking info.Control)
    info.Checker.FinishedChecking |> Event.add (fun _ -> updateCtlWaiting info.Control)
    info.Checker.Error |> Event.add (fun e -> updateCtlError info.Control; errors.Add(e))

form.Shown |> Event.add (fun _ -> 
    let items = 
        initFormContent listView 
        |> Seq.toList
        |> List.map (domap (fun info -> Controls.updateDates info))
        |> List.map (domap registerEvents)
    listView.Sort()

    updateBtn.Click |> Event.add (fun _ ->
        errorsTb.Text <- ""
        for info in items do 
            info.Control.BackColor <- Color.Gray
        async { 
            let results =
                items 
                |> List.filter (fun info -> info.Checker.GetLastCheckDate().AddMinutes(info.Definition.Interval) < DateTime.Now &&
                                            info.Definition.Enabled)
                |> List.map (fun info -> async {
                        let change = info.Checker.CheckChange()
                        do! Async.SwitchToContext(syncContext)
                        do! Async.SwitchToThreadPool()
                        return (info, change)
                    }) 
                |> Async.Parallel
                |> Async.RunSynchronously

            do! Async.SwitchToContext(syncContext)
            results
                |> Array.map (domap (fun (info, res) -> info.Control.BackColor <- Color.White
                                                        Controls.updateDates info))
                |> Array.filter snd
                |> Array.map fst
                |> Array.map (domap (fun info -> info.Control.BackColor <- Color.Yellow))
                |> Array.iter (fun info -> for v in info.Checker.ReportChangedValue() do
                                                info.Notifier errorsTb info.Checker v
                                               )
            listView.Sort()
            do! Async.SwitchToThreadPool()
        } |> Async.Start
    )
)
form.ShowDialog() |> ignore

// pozn: k prehazovani kontextu viz.
// http://stackoverflow.com/questions/5433397/f-purpose-of-switchtothreadpool-just-before-async-return
*)