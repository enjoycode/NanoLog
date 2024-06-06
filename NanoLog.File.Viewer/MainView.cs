using Terminal.Gui;

namespace NanoLog.File.Viewer;

public sealed class MainView : Toplevel
{
    private readonly ListView _filesListView;
    private readonly LogsTableView _logsTableView;
    private string _logsFolder = null!;
    private string _currentTheme = "Default";

    private static readonly LogsTableSource EmptyTableSource = new([]);

    public MainView()
    {
        ColorScheme = Colors.ColorSchemes["Base"];
        MenuBar = BuildMenuBar();
        StatusBar = BuildStatusBar();

        _filesListView = BuildFileListView();
        _logsTableView = BuildLogsTableView();

        Add(MenuBar);
        Add(_filesListView);
        Add(_logsTableView);
        Add(StatusBar);
    }

    #region ====UI Build====

    private MenuBar BuildMenuBar() => new()
    {
        Menus =
        [
            new MenuBarItem("_File",
            [
                new MenuItem("_Open", null, OnOpenFolder, null, null, KeyCode.O | KeyCode.CtrlMask),
                new MenuItem("_Quit", null, RequestStop, null, null, KeyCode.Q | KeyCode.CtrlMask),
            ]),
            new MenuBarItem("_Theme", CreateThemeMenuItems()),
            new MenuBarItem("_Help",
            [
                new MenuItem("_About...", null, ShowAbout, null, null)
            ]),
        ]
    };

    private MenuItem[] CreateThemeMenuItems()
    {
        var items = new List<MenuItem>();
        var schemeCount = 0;
        foreach (var theme in ConfigurationManager.Themes!)
        {
            var item = new MenuItem
            {
                Title = $"_{theme.Key}",
                Shortcut = (KeyCode)new Key((KeyCode)((uint)KeyCode.D1 + schemeCount++)).WithCtrl
            };
            item.CheckType |= MenuItemCheckStyle.Checked;
            item.Checked = theme.Key == _currentTheme;

            item.Action += () =>
            {
                ConfigurationManager.Themes.Theme = _currentTheme = theme.Key;
                items.ForEach(m => m.Checked = false);
                item.Checked = true;
                ConfigurationManager.Apply();
                Application.Top.SetNeedsDisplay();
            };
            items.Add(item);
        }

        return items.ToArray();
    }

    private static StatusBar BuildStatusBar() => new()
    {
        Visible = true,
        Items =
        [
            new StatusItem(KeyCode.CharMask, "enjoycode@icloud.com", null)
        ]
    };

    private ListView BuildFileListView()
    {
        var listView = new ListView()
        {
            X = 0, Y = 1,
            Width = 28,
            Height = Dim.Fill(1),
            AllowsMarking = false,
            AllowsMultipleSelection = false,
            CanFocus = true,
            Title = "Files",
            BorderStyle = LineStyle.Single,
            SuperViewRendersLineCanvas = true
        };
        //listView.OpenSelectedItem += (_, _) => _logsTableView.SetFocus();
        listView.SelectedItemChanged += OnSelectedFile;
        return listView;
    }

    private LogsTableView BuildLogsTableView()
    {
        var table = new LogsTableView()
        {
            X = Pos.Right(_filesListView), Y = 1,
            Width = Dim.Fill(0),
            Height = Dim.Fill(1),
            CanFocus = true,
            FullRowSelect = true,
            Table = EmptyTableSource,
            Title = "Logs",
            BorderStyle = LineStyle.Single,
            SuperViewRendersLineCanvas = true
        };
        table.Style.AlwaysShowHeaders = true;
        table.Style.ShowHorizontalBottomline = true;
        // table.Style.ShowHorizontalHeaderOverline = false;
        // table.Style.ShowHorizontalHeaderUnderline = false;
        // table.Style.ShowVerticalCellLines = false;
        // table.Style.ShowVerticalHeaderLines = false;

        return table;
    }

    private static void ShowAbout()
    {
        MessageBox.Query("About...", "EnjoyCode", "_Ok");
    }

    #endregion

    #region ====Event Hander====

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

    #endregion
}