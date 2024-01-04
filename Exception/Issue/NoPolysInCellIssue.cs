
using Godot;

public class NoPolysInCellIssue : Issue
{
    public Vector2I Key { get; private set; }
    public NoPolysInCellIssue(Vector2I key, Vector2 point, string message) : base(point, message)
    {
        Key = key;
    }

    public override void Draw(Client c)
    {
        var grid = c.Data.Planet.PolygonAux.MapPolyGrid;
        var debugDrawer = c.GetComponent<MapGraphics>().DebugOverlay;
        debugDrawer.Clear();
        debugDrawer.Draw(mb => mb.AddPoint(Vector2.Zero, 10f, Colors.Red), Point);
        var cellW = grid.CellWidth;
        var cellH = grid.CellHeight;
        var dim = new Vector2(cellW, cellH);
        var tl = Key * dim;
        var tr = (Key + Vector2I.Right) * dim;
        var bl = (Key + Vector2I.Down) * dim;
        var br = (Key + Vector2I.One) * dim;
        
        debugDrawer.DrawLine(tl, tr, Colors.Blue, 5f);
        debugDrawer.DrawLine(tl, bl, Colors.Blue, 5f);
        debugDrawer.DrawLine(br, tr, Colors.Blue, 5f);
        debugDrawer.DrawLine(br, bl, Colors.Blue, 5f);
    }
}