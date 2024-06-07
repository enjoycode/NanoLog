using System.Runtime.InteropServices;

namespace NanoLog.File.Viewer;

/// <summary>
/// 包装LogEvent及LogMessage传入表达式
/// </summary>
/// <remarks>Linq表达式不支持ref struct</remarks>
public readonly struct LogDataParameter
{
    public LogDataParameter(List<(LogEvent, LogMessage)> source, int row)
    {
        _source = source;
        _row = row;

        //TODO: cache result for search again.
        Visitor.BeforeVisit();
        Visitor.Visit(ref CollectionsMarshal.AsSpan(_source)[_row].Item2);
        _rootNode = Visitor.AfterVisit();
    }

    private static readonly LogTokenVisitor Visitor = new();

    private readonly List<(LogEvent, LogMessage)> _source;
    private readonly LogTokenNode _rootNode;
    private readonly int _row;

    public LogLevel Level => CollectionsMarshal.AsSpan(_source)[_row].Item1.Level;
    public DateTime Time => CollectionsMarshal.AsSpan(_source)[_row].Item1.Time;
    public string Category => CollectionsMarshal.AsSpan(_source)[_row].Item1.Category;
    public string File => CollectionsMarshal.AsSpan(_source)[_row].Item1.File;
    public string Member => CollectionsMarshal.AsSpan(_source)[_row].Item1.Member;
    public int Line => CollectionsMarshal.AsSpan(_source)[_row].Item1.Line;

    public LogTokenNode this[string name] => _rootNode[name];
}