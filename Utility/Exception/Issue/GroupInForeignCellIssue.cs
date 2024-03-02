
using Godot;

public class GroupInForeignCellIssue : Issue
{
    private Cell _cell;
    private UnitGroup _group;
    public GroupInForeignCellIssue(UnitGroup g, Cell cell) 
        : base(cell.GetCenter(), "")
    {
        _cell = cell;
        _group = g;
    }

    public override void Draw(Client c)
    {
        var mb = c.GetComponent<MapGraphics>();
        var debug = mb.DebugOverlay;
        debug.Clear();
        debug.Draw(mb =>
        {
            var groupP = Pos.Offset(_group.GetCell(c.Data).GetCenter(), c.Data);
            var cellP = Pos.Offset(_cell.GetCenter(), c.Data);
            mb.AddPoint(cellP, 20f, Colors.Black);
            mb.AddPoint(groupP, 10f, _group.Color);
        }, Pos);
    }
}