
using Godot;

public class GroupInForeignCellIssue : Issue
{
    private PolyCell _cell;
    private UnitGroup _group;
    public GroupInForeignCellIssue(UnitGroup g, PolyCell cell) 
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
            var groupP = Pos.GetOffsetTo(_group.GetCell(c.Data).GetCenter(), c.Data);
            var cellP = Pos.GetOffsetTo(_cell.GetCenter(), c.Data);
            mb.AddPoint(cellP, 20f, Colors.Black);
            mb.AddPoint(groupP, 10f, _group.Color);
        }, Pos);
    }
}