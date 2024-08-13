
using Godot;

public class NoPolysInCellIssue : Issue
{
    public Vector2I Key { get; private set; }
    public NoPolysInCellIssue(Vector2I key, 
        Vector2 point, string message, int tick) : base(point, message, tick)
    {
        Key = key;
        AddLayer("base", mb =>
        {
            var grid = Game.I.Client.Data.Planet.MapAux.MapPolyGrid;
            var debugDrawer = Game.I.Client.GetComponent<MapGraphics>()
                .GetOverlay(LayerOrder.Debug);
            debugDrawer.Clear();
            mb.AddSquare(Vector2.Zero, 10f, Colors.Red);
            var cellW = grid.CellWidth;
            var cellH = grid.CellHeight;
            var dim = new Vector2(cellW, cellH);
            var tl = Key * dim;
            var tr = (Key + Vector2I.Right) * dim;
            var bl = (Key + Vector2I.Down) * dim;
            var br = (Key + Vector2I.One) * dim;
        
            mb.AddLine(tl - tl, tr - tl, Colors.Blue, 5f);
            mb.AddLine(tl - tl, bl - tl, Colors.Blue, 5f);
            mb.AddLine(br - tl, tr - tl, Colors.Blue, 5f);
            mb.AddLine(br - tl, tr - bl, Colors.Blue, 5f);
        });
    }
}