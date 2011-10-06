module EventsChecker.UI.Form

open System
open System.Drawing
open System.Threading
open EventsChecker.Core
open System.Windows.Forms
open EventsChecker.UI
open NLog

type AltTabVisibility =
    | Visible
    | Invisible
    
let IconFileBig = "ico32.ico"
let IconFileSmall = "ico16.ico"
let logger = LogManager.GetLogger("mainprogram")

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

let mutable (form:Form) = null
let mutable (fakeParentForm:Form) = null
let mutable (notifyIcon: NotifyIcon) = null

let setFormVisibilityWhenAltTab visibility =
    match visibility with
    | Visible -> form.Owner <- null
    | Invisible -> form.Owner <- fakeParentForm

let initialize() =
    let syncContext = System.Threading.SynchronizationContext.Current
    let listView, infoTb, clearBtn = Controls.fillForm form

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
            syncContext.Post(new SendOrPostCallback(fun _ -> info.Control.BackColor <- Color.Yellow), ())
        let updateCtlWaiting ctl =
            syncContext.Post(new SendOrPostCallback(fun _ -> info.Control.BackColor <- Color.White), ())
        let updateCtlError ctl =
            syncContext.Post(new SendOrPostCallback(fun _ -> info.Control.BackColor <- Color.Red), ())

        info.Checker.StartedChecking  |> Event.add (fun _ -> updateCtlWorking info.Control)
        info.Checker.FinishedChecking |> Event.add (fun _ -> updateCtlWaiting info.Control
                                                             checkersHealth.RegisterSuccess(info))
        info.Checker.Error |> Event.add (fun e -> updateCtlError info.Control
                                                  checkersHealth.RegisterFailure(info))

    let runNotifications info =
        let changedValue = info.Checker.ReportChangedValue()
        let notificationInfo = { TextBox = infoTb
                                 NotifyIcon = notifyIcon
                                 Checker = info.Checker
                                 ChangeDescription = changedValue}
        for notifier in info.Notifier do
            notifier notificationInfo

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
                runNotifications info

            let! interval = checkersHealth.AsyncGetNextInterval(info)
            Controls.updateDates interval info

            listView.Sort()
            do! Async.SwitchToThreadPool()

            return! updateChecker info
        }

    let hideWindow() =
        if form.ShowInTaskbar = true then   // show in taskbar as indication whehter the window is visible or not   
            logger.Debug("hiding window")
            form.ShowInTaskbar <- false
            form.WindowState <- FormWindowState.Minimized
            setFormVisibilityWhenAltTab Invisible
    let restoreWindow() =
        if form.ShowInTaskbar = false then
            logger.Debug("restoring window")
            form.ShowInTaskbar <- true
            form.WindowState <- FormWindowState.Normal
            setFormVisibilityWhenAltTab Visible
    clearBtn.Click 
        |> Event.add (fun _ -> Notifier.notificationsHistory.Clear()
                               infoTb.Text <- "")
    notifyIcon.Click 
        |> Event.add (fun _ -> logger.Debug("notifyIcon.Click, {0}", form.WindowState)
                               if form.WindowState = FormWindowState.Minimized then
                                    restoreWindow()
                               else
                                    hideWindow()
        )
    form.Resize 
        |> Event.add (fun _ -> logger.Debug("form.Resize, {0}", form.WindowState)
                               if form.WindowState = FormWindowState.Minimized then
                                  hideWindow()
        )
    form.FormClosing 
        |> Event.add (fun args -> logger.Debug("form.FormClosing, {0}", form.WindowState))
    form.FormClosed
        |> Event.add (fun _ -> logger.Debug("form.FormClosed")
                               notifyIcon.Visible <- false
                               notifyIcon.Dispose()
        )
    form.Load 
        |> Event.add (fun _ -> 
            logger.Debug("form.Load")
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
    fakeParentForm <- new Form(FormBorderStyle = FormBorderStyle.FixedToolWindow,
                               ShowInTaskbar = false)

[<STAThread>]
do
    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
    Application.ThreadException |> Event.add (fun args ->
        logger.ErrorException("ThreadException", args.Exception)
        logger.Error(args.Exception)
    )
    AppDomain.CurrentDomain.UnhandledException |> Event.add (fun args ->
        match args.ExceptionObject with
        | :? Exception as e-> logger.ErrorException("AppDomainException", e)
                              logger.Error(e)        // check why ErrorException doesn't log exception e
        | _ -> ()
    )
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    
    form <- new System.Windows.Forms.Form(
                 Text = "Events Notifier",
                 Icon = new Icon(IconFileBig)
               )
    notifyIcon <- new NotifyIcon(
                    ContextMenuStrip = new ContextMenuStrip(),
                    Icon = new Icon(IconFileSmall),
                    Text = "This is Events notifier",
                    Visible = true)   //        notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
    initialize()
    Application.Run(form);
//form.Show() |> ignore

// pozn: k prehazovani kontextu viz.
// http://stackoverflow.com/questions/5433397/f-purpose-of-switchtothreadpool-just-before-async-return
// exceptions handling:
//  - http://msdn.microsoft.com/en-us/library/system.windows.forms.application.threadexception.aspx
//  - http://weblogs.asp.net/fmarguerie/archive/2005/04/21/403661.aspx
// ShowInTaskbar causes FormClosing... :| when run via ShowDialog
//   http://social.msdn.microsoft.com/Forums/en-US/vbgeneral/thread/d6a1b067-10f0-4bcd-bf3d-4e9b6b72a0a7    /
// some links to system tray icon
//  http://stackoverflow.com/questions/46918/whats-the-proper-way-to-minimize-to-tray-a-c-winforms-app
// idea with hidden window from ALT+Tab
//   http://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher
// icon: <link rel="shortcut icon" href="http://faviconist.com/icons/adcc3e1859e220b9ae957d92035df7fb/favicon.ico" />