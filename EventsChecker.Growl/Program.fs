open System

printfn "Registers Growl application.\n Press enter to continue"
System.Console.ReadLine() |> ignore

let registerApp appName name displayName = // (iconPath : string option) =
    let connector = Growl.Connector.GrowlConnector()
    let ttype = Growl.Connector.NotificationType(name, displayName)
    //if iconPath.IsSome then
    //  ttype.Icon <- new Growl.CoreLibrary.Resource(

    connector.Register(new Growl.Connector.Application(appName), [|ttype|])

registerApp "EventsChecker.UI" "EventsChecker.UI.NewEvent" "New Event"
Console.WriteLine("Application registered");

// try send
let notification =
    Growl.Connector.Notification(
        "EventsChecker.UI", 
        "EventsChecker.UI.NewEvent", //"New Event",
        System.DateTime.Now.Ticks.ToString(),
        "Registered",
        "Growl application registered")
let connector = Growl.Connector.GrowlConnector()
connector.Notify(notification)