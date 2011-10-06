namespace EventsChecker.UI

open System.Windows.Forms
open EventsChecker.Core

type NotificationInput = {
    TextBox : TextBox
    NotifyIcon : NotifyIcon
    Checker : IChecker
    ChangeDescription : CheckerChangedValue
}

type CheckerInfo = {
    Control : ListViewItem
    Checker : IChecker
    Notifier : (NotificationInput -> unit) list        // trying func instead of interface..
    Definition : CheckerDefinition
}
