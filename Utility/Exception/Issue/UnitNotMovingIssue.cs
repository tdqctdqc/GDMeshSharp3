
using System.Linq;
using Godot;

public class UnitNotMovingIssue : Issue
{
    public Vector2 Dest { get; private set; }
    public MoveType MoveType { get; private set; }
    public Regime Regime { get; private set; }
    public bool GoThruHostile { get; private set; }
    public UnitNotMovingIssue(Vector2 dest,
        Vector2 unitPos, string message, MoveType moveType,
        Regime regime, bool goThruHostile, int tick) : base(unitPos, 
        message, tick)
    {
        MoveType = moveType;
        Regime = regime;
        Dest = dest;
        GoThruHostile = goThruHostile;
    }

    private void Draw(Client c)
    {
        // var debugDrawer = c.GetComponent<MapGraphics>()
        //     .DebugOverlay;
        // debugDrawer.Clear();
        // var destRel = UnitPos.GetOffsetTo(Dest, c.Data);
        // debugDrawer.Draw(mb => mb.AddPoint(Vector2.Zero, 10f, Colors.Purple), UnitPos);
        // debugDrawer.Draw(mb => mb.AddPoint(destRel, 10f, Colors.Orange), UnitPos);
        // debugDrawer.Draw(mb => mb.AddLine(Vector2.Zero, destRel, Colors.Blue, 2f), UnitPos);
        //
        // var dist = destRel.Length();
        // var wps = c.Data.Military.WaypointGrid.GetWithin(UnitPos, dist * 2f, v => true);
        // foreach (var wp in wps)
        // {
        //     var passable = MoveType.Passable(wp, Alliance, c.Data);
        //     var color = passable ? Colors.Green : Colors.Red;
        //     var pRel = UnitPos.GetOffsetTo(wp.Pos, c.Data);
        //     debugDrawer.Draw(mb => mb.AddPoint(pRel, 5f, color), UnitPos);
        //     foreach (var nWp in wp.GetNeighbors(c.Data).Where(wps.Contains))
        //     {
        //         var nPRel = UnitPos.GetOffsetTo(nWp.Pos, c.Data);
        //         debugDrawer.Draw(mb => mb.AddLine(pRel, nPRel, color, 2f), UnitPos);
        //     }
        // }
    }
}