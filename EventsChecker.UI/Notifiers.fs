namespace EventsChecker.UI

open System
open System.Windows.Forms
open EventsChecker.Core
open System.Text.RegularExpressions

module Notifier =
    let textNotify input=
        let text = System.String.Join("\r\n", input.Messages)
        input.TextBox.Text <- input.TextBox.Text + text + "\r\n"

    let growlNotify input =
        let text = System.String.Join("\r\n", input.Messages)
        let notification =
            Growl.Connector.Notification(
                "EventsChecker.UI", 
                "EventsChecker.UI.NewEvent", 
                System.DateTime.Now.Ticks.ToString(),
                (sprintf "Event for %s" (input.Checker.ToString())),
                text,
                Priority = Growl.Connector.Priority.Normal)
        let connector = Growl.Connector.GrowlConnector()
        connector.Notify(notification)

    let notifyIconNotify delay input = 
        input.NotifyIcon.ShowBalloonTip(
                    delay, 
                    "Event notification", 
                    (sprintf "New event for %s" (input.Checker.ToString())), 
                    ToolTipIcon.Info)

    let poshScriptNotify script input =
        let executor = new EventsChecker.Notifier.PowerShellRunner(script)
        executor.Run(input.TextBox, input.Checker, input.Messages)

module NotifierParsers =
    let (|PoshRunner|_|) str =
        let regexMatch = Regex.Match(str, "PowerShell\((?<script>[\d\w\.\-]+\.ps1)\)")
        if regexMatch.Success then
            Some(regexMatch.Groups.["script"].Value)
        else
            None
    let (|NotifyIcon|_|) str =
        let regexMatch = Regex.Match(str, "NotifyIcon(\((?<delay>\d+)\)|$)")
        if regexMatch.Success then
            if regexMatch.Groups.["delay"].Success then
                Some(regexMatch.Groups.["delay"].Value |> int)
            else
                Some(2000)
        else
            None
    let private parseOneNotifier (str : string) =
        match str with
        | "Text" -> Notifier.textNotify
        | "Growl" -> Notifier.growlNotify
        | PoshRunner script -> Notifier.poshScriptNotify script
        | NotifyIcon delay -> Notifier.notifyIconNotify delay
        |_ -> failwith (sprintf "Unknown notifier %s" str)
    let parseNotifier (str:string) =
        str.Split(',') |> Seq.map parseOneNotifier |> Seq.toList
                