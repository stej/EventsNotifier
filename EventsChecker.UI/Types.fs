namespace EventsChecker.UI

open System.Windows.Forms
open EventsChecker.Core

type CheckerInfo = {
    Control : ListViewItem
    Checker : IChecker
    Notifier : (TextBox -> IChecker -> string list-> unit) list        // trying func instead of interface..
    Definition : CheckerDefinition
}
