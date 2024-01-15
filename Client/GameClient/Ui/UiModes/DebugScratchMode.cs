
using System.Linq;
using Godot;

public class DebugScratchMode : UiMode
{
    public DebugScratchMode(Client client) : base(client)
    {
    }

    public override void Process(float delta)
    {
    }

    private bool _drawn = false;
    public override void HandleInput(InputEvent e)
    {
        _client.Cam().HandleInput(e);
        if (_drawn) return;
        _drawn = true;
        var mg = _client.GetComponent<MapGraphics>();

        var cells = _client.Data.GetAll<PolyCells>().First();
        foreach (var c in cells.Cells)
        {
            var v = c.Vegetation.Model(_client.Data);
            var lf = c.Landform.Model(_client.Data);
            var vegCol = v.Color.Darkened(lf.DarkenFactor);
            var col = ColorsExt.GetRandomColor();
            mg.DebugOverlay.Draw(mb =>
            {
                var tris = Geometry2D.TriangulatePolygon(c.RelBoundary);
                for (var i = 0; i < tris.Length; i+=3)
                {
                    var p1 = c.RelBoundary[tris[i]];
                    var p2 = c.RelBoundary[tris[i+1]];
                    var p3 = c.RelBoundary[tris[i+2]];
                    // mb.AddTri(p1, p2, p3, lf.Color);
                    // mb.AddTri(p1, p2, p3, v.Color);
                    mb.AddTri(p1, p2, p3, col);
                }
            }, c.RelTo);
        }
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
        _drawn = false;
    }
}