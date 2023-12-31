using Godot;


public class NoPolyAtPointIssue : Issue
{
    public MapPolygon FoundPoly { get; set; }

    public override void Draw(Client c)
    {
        var highlighter = c.GetComponent<MapGraphics>()
            .Highlighter;
        highlighter.Clear();
        highlighter.DrawPoly(FoundPoly, c.Data);
        highlighter.DrawPoint(Point, 10f, Colors.Red);
        highlighter.DrawLine(FoundPoly.Center, Point, 5f, Colors.Blue);
    }

    public NoPolyAtPointIssue(Vector2 point, MapPolygon foundPoly,
        string message) : base(point, message)
    {
        FoundPoly = foundPoly;
    }
}