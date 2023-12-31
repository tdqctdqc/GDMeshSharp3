
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
        var highlighter = c.GetComponent<MapGraphics>().Highlighter;
        highlighter.Clear();
        highlighter.DrawPoint(Point, 10f, Colors.Red);
        var cellW = grid.CellWidth;
        var cellH = grid.CellHeight;
        var dim = new Vector2(cellW, cellH);
        var tl = Key * dim;
        var tr = (Key + Vector2I.Right) * dim;
        var bl = (Key + Vector2I.Down) * dim;
        var br = (Key + Vector2I.One) * dim;
        
        highlighter.DrawLine(tl, tr, 5f, Colors.Blue);
        highlighter.DrawLine(tl, bl, 5f, Colors.Blue);
        highlighter.DrawLine(br, tr, 5f, Colors.Blue);
        highlighter.DrawLine(br, bl, 5f, Colors.Blue);
    }
}