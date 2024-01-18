
using System.Linq;
using Godot;

public class TacticalMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public TacticalMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler();
    }

    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        Game.I.Client.Cam().HandleInput(e);
        _mouseOverHandler.Process(_client.Data, mapPos);
        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }
        
        var debugDrawer 
            = Game.I.Client.GetComponent<MapGraphics>()
                .DebugOverlay;
        debugDrawer.Clear();
        
        DrawFrontSegment();
        Tooltip(mapPos);
    }

    public override void Clear()
    {
    }
    
    private void DrawFrontSegment()
    {
        var debugDrawer 
            = Game.I.Client.GetComponent<MapGraphics>()
                .DebugOverlay;
        if (_mouseOverHandler.MouseOverCell == null)
        {
            return;
        }
        var cell = _mouseOverHandler.MouseOverCell;
        var regimes = _client.Data.HostLogicData.AllianceAis.Dic.Values
            .SelectMany(v => v.MilitaryAi.AreasOfResponsibility
                .Where(kvp =>
                    kvp.Value.Contains(_mouseOverHandler.MouseOverCell)))
            .Select(kvp => kvp.Key);
        foreach (var regime in regimes)
        {
            var ai = _client.Data.HostLogicData.RegimeAis[regime];
            var theater = ai.Military.Deployment.ForceAssignments.OfType<TheaterAssignment>()
                .FirstOrDefault(t => t.HeldCellIds.Contains(cell.Id));
            if (theater == null) continue;
            var front = theater.Assignments
                .OfType<FrontAssignment>()
                .FirstOrDefault(f => f.HeldCellIds.Contains(cell.Id));
            if (front == null) continue;
            var seg = front.Assignments.OfType<FrontSegmentAssignment>()
                .FirstOrDefault(s => s.FrontLineCellIds.Contains(cell.Id));
            if (seg == null) continue;
            var relTo = seg.GetCells(_client.Data).First().GetCenter();
            debugDrawer.Draw(mb => mb.DrawFrontSegment(relTo, seg, _client.Data), relTo);
        }
    }

    private void Tooltip(Vector2 mapPos)
    {
        
    }
}