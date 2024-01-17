
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
        Highlight();
        return;
        if (_drawn) return;
        _drawn = true;
        var mg = _client.GetComponent<MapGraphics>();

        var cells = _client.Data.GetAll<PolyCells>().First();
        
        foreach (var c in cells.Cells.Values)
        {
            var v = c.Vegetation.Model(_client.Data);
            var lf = c.Landform.Model(_client.Data);
            var vegCol = v.Color.Darkened(lf.DarkenFactor);
            var col = ColorsExt.GetRandomColor();
            mg.DebugOverlay.Draw(mb =>
            {
                mb.DrawPolygon(c.RelBoundary, col);
            }, c.RelTo);
        }
    }

    private void Highlight()
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
        _drawn = false;
    }
}