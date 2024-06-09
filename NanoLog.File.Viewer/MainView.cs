using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace NanoLog.File.Viewer;

public sealed class MainView : Toplevel
{
    private readonly ListView _filesListView;
    private readonly ListView _logsListView;
    private string _logsFolder = null!;
    private readonly RecordReader _logsReader = new();

    public MainView()
    {
        ReplaceKeyBinding(Key.CtrlMask | Key.F, Key.CtrlMask | Key.N);

        ColorScheme = Colors.ColorSchemes["Base"];
        MenuBar = BuildMenuBar();
        StatusBar = BuildStatusBar();

        _filesListView = BuildFileListView();
        _logsListView = BuildLogsListView();

        var letPanel = BuildLeftPanel();
        letPanel.Add(_filesListView);
        var rightPanel = BuildRightPanel(letPanel);
        rightPanel.Add(_logsListView);

        Add(MenuBar);
        Add(letPanel);
        Add(rightPanel);
        Add(StatusBar);
    }

    #region ====UI Build====

    private MenuBar BuildMenuBar() => new()
    {
        Menus =
        [
            new MenuBarItem("_File",
            [
                new MenuItem("_Open", null, OnOpenFolder, null, null, Key.CtrlMask | Key.O),
                new MenuItem("_Quit", null, RequestStop, null, null, Key.CtrlMask | Key.Q),
            ]),
            new MenuBarItem("_Logs",
            [
                new MenuItem("_Find", null, OnSearch, null, null, Key.CtrlMask | Key.F),
            ]),
            new MenuBarItem("_Help",
            [
                new MenuItem("_About...", null, ShowAbout, null, null)
            ]),
        ]
    };

    private static StatusBar BuildStatusBar() => new()
    {
        Visible = true,
        Items =
        [
            new StatusItem(Key.CharMask, "enjoycode@icloud.com", null)
        ]
    };

    private static FrameView BuildLeftPanel()
    {
        var panel = new FrameView
        {
            X = 0, Y = 1, Width = 28, Height = Dim.Fill(1),
            CanFocus = true,
            Title = "Files",
            Shortcut = Key.CtrlMask | Key.D
        };
        panel.Title = $"{panel.Title} ({panel.ShortcutTag})";
        panel.ShortcutAction = () => panel.SetFocus();
        return panel;
    }

    private static FrameView BuildRightPanel(FrameView leftPanel)
    {
        var panel = new FrameView
        {
            X = Pos.Right(leftPanel), Y = 1, Width = Dim.Fill(0), Height = Dim.Fill(1),
            CanFocus = true,
            Title = "Logs",
            Shortcut = Key.CtrlMask | Key.L
        };
        panel.Title = $"{panel.Title} ({panel.ShortcutTag})";
        panel.ShortcutAction = () => panel.SetFocus();
        return panel;
    }

    private ListView BuildFileListView()
    {
        var listView = new ListView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(0),
            Height = Dim.Fill(0),
            AllowsMarking = false,
            AllowsMultipleSelection = false,
            CanFocus = true,
        };
        //listView.OpenSelectedItem += (_, _) => _logsTableView.SetFocus();
        listView.SelectedItemChanged += OnSelectedFile;
        return listView;
    }

    private static ListView BuildLogsListView() => new()
    {
        X = 0, Y = 0,
        Width = Dim.Fill(0),
        Height = Dim.Fill(0),
        AllowsMarking = false,
        AllowsMultipleSelection = false,
        CanFocus = true,
    };

    private static void ShowAbout()
    {
        MessageBox.Query("About...", "EnjoyCode", "_Ok");
    }

    #endregion

    #region ====Event Hander====

    private void OnOpenFolder()
    {
        using var dlg = new OpenDialog("Open Logs Folder", null, null, OpenDialog.OpenMode.Directory);
        Application.Run(dlg);
        if (dlg.Canceled)
            return;

        if (string.IsNullOrEmpty(dlg.FilePaths[0]))
            return;
        _logsFolder = dlg.FilePaths[0];

        var files = Directory.EnumerateFiles(_logsFolder, "*-*-*.log")
            .Select(f => Path.GetFileName(f)!)
            .ToArray();
        _filesListView.SetSource(files);
        if (files.Length > 0)
            _filesListView.SelectedItem = 0;
    }

    private void OnSelectedFile(ListViewItemEventArgs e)
    {
        if (e.Item < 0) return;

        LogList list;
        try
        {
            list = _logsReader.ReadLogs(Path.Combine(_logsFolder, e.Value.ToString()!));
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Read Log File Error", ex.Message);
            return;
        }

        // _logsTableView.Table = new LogsTableSource(logsReader.AllRecords);
        _logsListView.Source = new LogsDataSource(list);
    }

    private void OnSearch()
    {
        using var dlg = new Dialog("Find...");
        dlg.Width = 66;
        dlg.Height = 8;

        var attribute = Attribute.Make(Color.Black, Color.White);
        var colorScheme = new ColorScheme
        {
            Normal = attribute,
            Focus = attribute,
            HotNormal = attribute,
            HotFocus = attribute,
            Disabled = attribute
        };

        var input = new TextView()
        {
            X = 0, Y = 0,
            Width = Dim.Width(dlg) - 2,
            Height = Dim.Height(dlg) - 3,
            CanFocus = true,
            ColorScheme = colorScheme
        };
        dlg.Add(input);

        var clearButton = new Button("Clear");
        clearButton.Clicked += () =>
        {
            input.Text = string.Empty;
            input.SetFocus();
        };
        var cancelButton = new Button("Cancel");
        cancelButton.Clicked += () => Application.RequestStop();
        var searchButton = new Button("Search", true);

        dlg.AddButton(clearButton);
        dlg.AddButton(cancelButton);
        dlg.AddButton(searchButton);

        dlg.Loaded += () => input.SetFocus();
        Application.Run(dlg);
    }

    #endregion
}