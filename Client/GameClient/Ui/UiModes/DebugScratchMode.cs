
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
        Highlight();
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
    }
}