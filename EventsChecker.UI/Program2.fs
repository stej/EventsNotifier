module EventsChecker.UI.Form

open System
open System.Drawing
open System.Threading
open EventsChecker.Core
open System.Windows.Forms
open EventsChecker.UI

let domap fce item =
    fce item
    item

let checkersHealth = new CheckersHealth()

let checkersDefinition = 
    DefinitionParser.parse "checkers.txt" 
    |> Seq.toList

let initFormContent (lv:ListView) =
     checkersDefinition 
     |> List.map (fun definition -> { Checker = definition.Checker
                                      Control = Controls.addChecker lv definition.Interval definition
                                      Notifier = NotifierParsers.parseNotifier definition.NotifierTypes
                                      Definition = definition})

let form = new System.Windows.Forms.Form(Text = "test")
let syncContext = System.Threading.SynchronizationContext.Current
let listView, infoTb, updateBtn = Controls.fillForm form

listView.ListViewItemSorter <- 
    { new System.Collections.IComparer with 
        member this.Compare(x, y) = 
            let item1, item2 = x:?>ListViewItem, y:?>ListViewItem
            let def1, def2 = item1.Tag :?> CheckerDefinition, item2.Tag :?> CheckerDefinition
            let date1 = if def1.Enabled then Controls.getNextUpdateDate item1 else DateTime.MaxValue
            let date2 = if def2.Enabled then Controls.getNextUpdateDate item2 else DateTime.MaxValue
            DateTime.Compare(date1, date2)
    }

let registerEvents info =
    let updateCtlWorking ctl =
        syncContext.Post(new SendOrPostCallback(fun _ -> Controls.setRowState Controls.Working ctl
                                                         info.Control.BackColor <- Color.Yellow), ())
    let updateCtlWaiting ctl =
        syncContext.Post(new SendOrPostCallback(fun _ -> Controls.setRowState Controls.Waiting ctl
                                                         info.Control.BackColor <- Color.White), ())
    let updateCtlError ctl =
        syncContext.Post(new SendOrPostCallback(fun _ -> Controls.setRowState Controls.Error ctl
                                                         info.Control.BackColor <- Color.Red), ())

    info.Checker.StartedChecking  |> Event.add (fun _ -> updateCtlWorking info.Control)
    info.Checker.FinishedChecking |> Event.add (fun _ -> updateCtlWaiting info.Control
                                                         checkersHealth.RegisterSuccess(info))
    info.Checker.Error |> Event.add (fun e -> updateCtlError info.Control
                                              checkersHealth.RegisterFailure(info))

let rec updateChecker (info:CheckerInfo) =
    async { 
        let! interval = checkersHealth.AsyncGetNextInterval(info)
        let timeToSleep = 
            if CheckersHealth.IsInfiniteInterval(interval) then
                TimeSpan.FromDays(1.).TotalMilliseconds
            else
                let lastCheckDate = info.Checker.GetLastCheckDate()
                let diff = lastCheckDate.AddMinutes(interval) - DateTime.Now
                Math.Max(0., diff.TotalMilliseconds)
        do! Async.Sleep(int timeToSleep)
        
        let change = info.Checker.CheckChange()

        do! Async.SwitchToContext(syncContext)
        if change then
            for notifier in info.Notifier do
                notifier infoTb info.Checker (info.Checker.ReportChangedValue())

        let! interval = checkersHealth.AsyncGetNextInterval(info)
        Controls.updateDates interval info

        listView.Sort()
        do! Async.SwitchToThreadPool()

        return! updateChecker info
    }
updateBtn.Click 
    |> Event.add (fun _ -> infoTb.Text <- "")
form.Shown 
    |> Event.add (fun _ -> 
        let items = initFormContent listView 
        for info in items do
            let interval = checkersHealth.GetNextInterval(info)
            Controls.updateDates interval info
            registerEvents info

        listView.Sort()
        async {
            items 
            |> List.filter (fun info -> info.Definition.Enabled)
            |> List.map updateChecker
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
        } |> Async.Start
    )
form.ShowDialog() |> ignore

// pozn: k prehazovani kontextu viz.
// http://stackoverflow.com/questions/5433397/f-purpose-of-switchtothreadpool-just-before-async-return