namespace EventsChecker.UI

open System
open System.Windows.Forms
open EventsChecker.Core
open System.Text.RegularExpressions

module Notifier =
    let textNotify (tb : TextBox) (checker:IChecker) (messages : string list)=
        let text = System.String.Join("\r\n", messages)
        tb.Text <- tb.Text + text + "\r\n"

    let growlNotify (tb : TextBox) (checker:IChecker) (messages : string list)=
        let text = System.String.Join("\r\n", messages)
        let notification =
            Growl.Connector.Notification(
                "EventsChecker.UI", 
                "EventsChecker.UI.NewEvent", 
                System.DateTime.Now.Ticks.ToString(),
                (sprintf "Event for %s" (checker.ToString())),
                text,
                Priority = Growl.Connector.Priority.Normal)
        let connector = Growl.Connector.GrowlConnector()
        connector.Notify(notification)

    let poshScriptNotify script (tb : TextBox) (checker:IChecker) (messages : string list)=
        let executor = new EventsChecker.Notifier.PowerShellRunner(script)
        executor.Run(tb, checker, messages)

module NotifierParsers =
    let (|PoshRunner|_|) str =
        let regexMatch = Regex.Match(str, "PowerShell\((?<script>[\d\w\.\-]+\.ps1)\)")
        if regexMatch.Success then
            Some(regexMatch.Groups.["script"].Value)
        else
            None
    let private parseOneNotifier (str : string) =
        match str with
        | "Text" -> Notifier.textNotify
        | "Growl" -> Notifier.growlNotify
        | PoshRunner script -> Notifier.poshScriptNotify script
        |_ -> failwith (sprintf "Unknown notifier %s" str)
    let parseNotifier (str:string) =
        str.Split(',') |> Seq.map parseOneNotifier |> Seq.toList
                