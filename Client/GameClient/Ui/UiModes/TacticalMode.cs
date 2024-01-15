
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
        if (_mouseOverHandler.MouseOverWaypoint == null)
        {
            return;
        }
        var wp = _mouseOverHandler.MouseOverWaypoint;
        var regimes = _client.Data.HostLogicData.AllianceAis.Dic.Values
            .SelectMany(v => v.MilitaryAi.AreasOfResponsibility
                .Where(kvp =>
                    kvp.Value.Contains(_mouseOverHandler.MouseOverWaypoint)))
            .Select(kvp => kvp.Key);
        foreach (var regime in regimes)
        {
            var ai = _client.Data.HostLogicData.RegimeAis[regime];
            var theater = ai.Military.Deployment.ForceAssignments.OfType<TheaterAssignment>()
                .FirstOrDefault(t => t.TacWaypointIds.Contains(wp.Id));
            if (theater == null) continue;
            var front = theater.Assignments
                .OfType<FrontAssignment>()
                .FirstOrDefault(f => f.HeldWaypointIds.Contains(wp.Id));
            if (front == null) continue;
            var seg = front.Assignments.OfType<FrontSegmentAssignment>()
                .FirstOrDefault(s => s.FrontLineWpIds.Contains(wp.Id));
            if (seg == null) continue;
            var relTo = seg.GetWaypoints(_client.Data).First().Pos;
            debugDrawer.Draw(mb => mb.DrawFrontSegment(relTo, seg, _client.Data), relTo);
        }
    }

    private void Tooltip(Vector2 mapPos)
    {
        var tooltip = _client.GetComponent<TooltipManager>();
        if (_mouseOverHandler.MouseOverWaypoint != null)
        {
            var template = new TacWaypointTooltipTemplate();
            tooltip.PromptTooltip(template, _mouseOverHandler.MouseOverWaypoint);
        }
        else
        {
            tooltip.Clear();
        }
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
    }

    private void PaintOccupation(Vector2 mapPos)
    {
        var found = _client.Data.Military.WaypointGrid.TryGetClosest(mapPos, out var close, wp => true);
        if (found)
        {
            var regime = _client.Data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(_client.Data);

            _client.Data.Military.TacticalWaypoints.OccupierRegimes[close.Id] = regime.Id;
            foreach (var n in close.GetNeighbors(_client.Data))
            {
                if (n is IWaterWaypoint) continue;
                _client.Data.Military.TacticalWaypoints.OccupierRegimes[n.Id] = regime.Id;
            }
        }
    }
}