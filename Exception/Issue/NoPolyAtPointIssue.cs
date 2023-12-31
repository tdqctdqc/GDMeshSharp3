using Godot;


public class NoPolyAtPointIssue : Issue
{
    public MapPolygon FoundPoly { get; set; }

    public override void Draw(Client c)
    {
        var debugDrawer = c.GetComponent<MapGraphics>()
            .DebugOverlay;
        debugDrawer.Clear();
        debugDrawer.Draw(mb => mb.DrawPolyBorders(FoundPoly.Center, FoundPoly, c.Data),
            FoundPoly.Center);
        debugDrawer.Draw(mb => mb.AddLine(Vector2.Zero, FoundPoly.Center.GetOffsetTo(UnitPos, c.Data), Colors.Blue, 5f),
            FoundPoly.Center);
    }

    public NoPolyAtPointIssue(Vector2 unitPos, MapPolygon foundPoly,
        string message) : base(unitPos, message)
    {
        FoundPoly = foundPoly;
    }
}