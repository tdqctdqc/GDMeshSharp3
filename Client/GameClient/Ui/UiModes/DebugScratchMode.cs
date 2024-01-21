
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

    public override void HandleInput(InputEvent e)
    {
        _client.Cam().HandleInput(e);
        HighlightCellAndAdjacent();
        // HighlightBoundaryCells();
    }

    private void HighlightBoundaryCells()
    {
        var highlighter = _client.GetComponent<MapGraphics>().Highlighter;
        highlighter.Clear();
        
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        var poly = _client.Data.Planet.PolygonAux.MapPolyGrid
            .GetElementAtPoint(mapPos, _client.Data);
        if (poly == null) return;
        var bSegs = poly.GetEdges(_client.Data)
            .SelectMany(e => e.GetSegsRel(poly, _client.Data).Segments);
        var bCells = poly
            .GetCells(_client.Data)
            .Where(c => c.RelBoundary
                .Any(p => bSegs.Any(s => s.DistanceTo(p) < .1f)));
        foreach (var lineSegment in bSegs)
        {
            
            highlighter.Draw(mb =>
                mb.AddLine(lineSegment.From, lineSegment.To, 
                    Colors.Blue, 10f), poly.Center);
        }
        foreach (var bCell in bCells)
        {
            highlighter.Draw(mb =>
                mb.DrawPolygon(bCell.RelBoundary, 
                    Colors.Red.GetPeriodicShade(bCell.Id)), bCell.RelTo);
        }

    }
    private void HighlightCellAndAdjacent()
    {
        var highlighter = _client.GetComponent<MapGraphics>().Highlighter;
        highlighter.Clear();
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
    }
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
    }
}