using System.Drawing;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace NanoLog.File.Viewer;

internal sealed class LogsTableView : TableView
{
    private int _col;
    private int _row;
    
    public override void OnDrawContent(Rectangle viewport)
    {
        _col = 0;
        _row = 0;
        base.OnDrawContent(viewport);
    }

    protected override void RenderCell(Attribute cellColor, string render, bool isPrimaryCell)
    {
        base.RenderCell(cellColor, render, isPrimaryCell);
    }
}