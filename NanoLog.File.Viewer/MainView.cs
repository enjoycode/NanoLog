using Terminal.Gui;

namespace NanoLog.File.Viewer;

public sealed class MainView : Toplevel
{
    private const int LeftPanelWidth = 28;
    private readonly FrameView _leftPanel;
    private readonly FrameView _rightPanel;
    private readonly ListView _filesListView;
    private readonly LogsTableView _logsTableView;

    private string _logsFolder = null!;

    private static readonly LogsTableSource EmptyTableSource = new([]);

    public MainView()
    {
        ColorScheme = Colors.ColorSchemes["Base"];
        MenuBar = BuildMenuBar();
        StatusBar = BuildStatusBar();

        _filesListView = BuildFileListView();
        _leftPanel = BuildLeftPanel();
        _leftPanel.Add(_filesListView);
        _logsTableView = BuildLogsTableView();
        _rightPanel = BuildRightPanel();
        _rightPanel.Add(_logsTableView);

        Add(MenuBar);
        Add(_leftPanel);
        Add(_rightPanel);
        Add(StatusBar);
    }

    private MenuBar BuildMenuBar() => new()
    {
        Menus =
        [
            new MenuBarItem("_File",
            [
                new MenuItem("_Open", null, OnOpenFolder, null, null, KeyCode.O | KeyCode.CtrlMask),
                new MenuItem("_Quit", null, RequestStop, null, null, KeyCode.Q | KeyCode.CtrlMask),
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
            new StatusItem(KeyCode.CharMask, "enjoycode@icloud.com", null)
        ]
    };

    private static FrameView BuildLeftPanel()
    {
        var leftPanel = new FrameView()
        {
            Title = "Files",
            X = 0,
            Y = 1,
            Width = LeftPanelWidth,
            Height = Dim.Fill(1),
            CanFocus = true,
            // HotKey = Key.F.WithCtrl,
        };
        // leftPanel.Title = $"{leftPanel.Title} ({leftPanel.HotKey})";

        return leftPanel;
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
            CanFocus = true
        };
        listView.OpenSelectedItem += (_, _) => _rightPanel.SetFocus();
        listView.SelectedItemChanged += OnSelectedFile;
        return listView;
    }

    private static FrameView BuildRightPanel()
    {
        var rightPane = new FrameView()
        {
            Title = "Logs",
            X = LeftPanelWidth,
            Y = 1, // for menu
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = true,
            // HotKey = Key.L.WithCtrl
        };
        // rightPane.Title = $"{rightPane.Title} ({rightPane.HotKey})";

        return rightPane;
    }

    private LogsTableView BuildLogsTableView()
    {
        return new LogsTableView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(0),
            Height = Dim.Fill(0),
            CanFocus = true,
            FullRowSelect = true,
            Table = EmptyTableSource,
        };
    }

    private static void ShowAbout()
    {
        MessageBox.Query("About...", "EnjoyCode", "_Ok");
    }

    private void OnOpenFolder()
    {
        var dlg = new OpenDialog()
        {
            Title = "Open Logs Folder",
            OpenMode = OpenMode.Directory
        };
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

    private void OnSelectedFile(object? sender, ListViewItemEventArgs e)
    {
        if (e.Item < 0) return;

        using var logsReader = new RecordReader(Path.Combine(_logsFolder, e.Value.ToString()!));
        try
        {
            logsReader.ReadAll();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Read Log File Error", ex.Message);
            return;
        }

        var tableSource = new LogsTableSource(logsReader.AllRecords);
        _logsTableView.Table = tableSource;
    }
}