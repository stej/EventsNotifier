module EventsChecker.UI.Controls

open System.Windows.Forms
open EventsChecker.Core
open EventsChecker.UI

let getErrorsTextBox() =
    new TextBox(
        Dock = DockStyle.Fill,
        Location = new System.Drawing.Point(0, 0),
        Name = "errors",
        Multiline = true,
        TabIndex = 1,
        ScrollBars = ScrollBars.Vertical
        )
let private getListView () =
    let colEnabled = new ColumnHeader(Text = "Enabled")
    let colName = new ColumnHeader(Text = "Name")
    let colDescription = new ColumnHeader(Text = "Description")

    let l = new ListView(
                Dock = DockStyle.Fill,
                Location = new System.Drawing.Point(0, 0),
                Name = "items",
                TabIndex = 0,
                View = View.Details,
                CheckBoxes = true)
    l.Columns.Add("Name", 150, HorizontalAlignment.Left) |> ignore
    l.Columns.Add("Interval", 30, HorizontalAlignment.Left) |> ignore
    l.Columns.Add("NextCheck", 120, HorizontalAlignment.Left) |> ignore
    l.Columns.Add("Description", 400, HorizontalAlignment.Left) |> ignore
    l

let private fillSplitter ()  =
    let s = new SplitContainer(
                Dock = DockStyle.Top,
                Location = new System.Drawing.Point(0, 0),
                Orientation = Orientation.Horizontal,
                Size = new System.Drawing.Size(600, 500),
                SplitterDistance = 400,
                Anchor = (AnchorStyles.Left ||| AnchorStyles.Top ||| AnchorStyles.Right ||| AnchorStyles.Bottom))
    let listView = getListView()
    let tb = getErrorsTextBox()
    s.Panel1.Controls.Add(listView)
    s.Panel2.Controls.Add(tb)
    s, listView, tb

let private getClearBtn() =
    new Button(Text = "Clear",
        Size = new System.Drawing.Size(600, 20),
        Location = new System.Drawing.Point(0, 500),
        Anchor = (AnchorStyles.Left ||| AnchorStyles.Right ||| AnchorStyles.Bottom))

let addChecker (listView: ListView) (interval: float) (def:CheckerDefinition)  =
    let item = new ListViewItem(Checked = def.Enabled,
                                Text = def.Checker.GetType().Name)
    let lastCheck = def.Checker.GetLastCheckDate()
    item.SubItems.Add(interval.ToString()) |> ignore
    item.SubItems.Add(lastCheck.AddMinutes(interval).ToString("yyyy-MM-dd HH:mm:ss")) |> ignore
    item.SubItems.Add(def.Checker.ToString()) |> ignore
    item.Tag <- def
    listView.Items.Add(item) |> ignore
    item

let IndefiniteUpdateDateStr = "-"
let updateDates (interval: float) (info: CheckerInfo) =
    let lastCheck = info.Checker.GetLastCheckDate()
    if CheckersHealth.IsInfiniteInterval(interval) then
        info.Control.SubItems.[1].Text <- "->"
        info.Control.SubItems.[2].Text <- IndefiniteUpdateDateStr
     else 
        info.Control.SubItems.[1].Text <- (int interval).ToString()
        info.Control.SubItems.[2].Text <- lastCheck.AddMinutes(interval).ToString("yyyy-MM-dd HH:mm:ss")

let getNextUpdateDate (item:ListViewItem) =
    let text = item.SubItems.[2].Text
    if text = IndefiniteUpdateDateStr then
        System.DateTime.MaxValue
    else
        System.DateTime.Parse(text)

let fillForm (form: Form) =
    form.ClientSize <- new System.Drawing.Size(600, 520)
    let splitter, listview, tb = fillSplitter()
    let updatebtn = getClearBtn()
    form.Controls.Add(splitter)
    form.Controls.Add(updatebtn)
    listview, tb, updatebtn