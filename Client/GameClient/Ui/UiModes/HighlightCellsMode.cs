
using System.Linq;
using Godot;

public class HighlightCellsMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public HighlightCellsMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedPoly += p => Highlight();
        _mouseOverHandler.ChangedCell += c => Highlight();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
    }

    private void Highlight()
    {
        var highlighter = _client.GetComponent<MapGraphics>().Highlighter;
        highlighter.Clear();
        HighlightCellAndAdjacent();
        HighlightPolyBorder();
    }
    
    private void HighlightPolyBorder()
    {
        var highlighter = _client.GetComponent<MapGraphics>().Highlighter;
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null) return;

        if (cell is ISinglePolyCell s)
        {
            var poly = s.Polygon.Entity(_client.Data);
            drawPoly(poly);
        }
        else if (cell is IEdgeCell e)
        {
            var p1 = e.Edge.Entity(_client.Data)
                .HighPoly.Entity(_client.Data);
            var p2 = e.Edge.Entity(_client.Data)
                .LowPoly.Entity(_client.Data);
            drawPoly(p1);
            drawPoly(p2);
        }

        void drawPoly(MapPolygon poly)
        {
            highlighter.Draw(mb =>
            {
                var ps = poly.BoundaryPoints;
                for (var i = 0; i < ps.Length; i++)
                {
                    var from = ps[i];
                    var to = ps.Modulo(i + 1);
                    mb.AddLine(from, to, Colors.Black, 3f);
                }
            }, poly.Center);
        }
    }
    private void HighlightCellAndAdjacent()
    {
        var highlighter = _client.GetComponent<MapGraphics>().Highlighter;
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        var cell = _client.Data.Planet.PolygonAux.PolyCellGrid
            .GetElementAtPoint(mapPos, _client.Data);

        
        highlighter.Draw(mb =>
            mb.DrawPolygon(cell.RelBoundary, Colors.Red), cell.RelTo);
        
        var cells = _client.Data.GetAll<PolyCells>()
            .First().Cells;
        
        foreach (var nId in cell.Neighbors)
        {
            var nCell = cells[nId];
            var col = Colors.Blue.Darkened(Game.I.Random.RandfRange(0f, .5f));
            highlighter.Draw(mb =>
                mb.DrawPolygon(nCell.RelBoundary, 
                    col), nCell.RelTo);
        }
        highlighter.Label(cell.Id.ToString(),
            Colors.White, cell.GetCenter(), .2f);
        foreach (var nId in cell.Neighbors)
        {
            var nCell = cells[nId];
            highlighter.Label(nCell.Id.ToString(),
                Colors.White, nCell.GetCenter(), .2f);
        }
    }
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
    }
}