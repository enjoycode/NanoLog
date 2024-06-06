// using Terminal.Gui;
//
// namespace NanoLog.File.Viewer;
//
// internal sealed class LogsTableSource : ITableSource
// {
//     public LogsTableSource(List<(LogEvent, LogMessage)> list)
//     {
//         _list = list;
//     }
//
//     private readonly List<(LogEvent, LogMessage)> _list;
//
//     public string[] ColumnNames => ["Time", "Level", "Category", "File", "Member", "Line", "Message"];
//     public int Columns => ColumnNames.Length;
//     public int Rows => _list.Count;
//
//     public object this[int row, int col] => col switch
//     {
//         0 => _list[row].Item1.Time,
//         1 => _list[row].Item1.Level,
//         2 => _list[row].Item1.Category,
//         3 => Path.GetFileName(_list[row].Item1.File),
//         4 => _list[row].Item1.Member,
//         5 => _list[row].Item1.Line,
//         _ => string.Empty //TODO:
//     };
// }