
using System;
using Godot;

public class NormalMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public NormalMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler();
    }
    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        var mapPos = Game.I.Client.Cam().GetMousePosInMapSpace();
        Game.I.Client.Cam().Process(e);
        _mouseOverHandler.Process(_client.Data, mapPos);

        if(e.IsAction("Open Regime Overview"))
        {
            TryOpenRegimeOverview();
        }
        if (e is InputEventKey k && k.Keycode == Key.R && k.Pressed == false)
        {
            PaintOccupation(mapPos);
        }
        Highlight(mapPos);
        Tooltip(mapPos);
    }

    private void Highlight(Vector2 mapPos)
    {
        var highlighter = _client.GetComponent<MapGraphics>().Highlighter;
        highlighter.Clear();
        if (_mouseOverHandler.MouseOverTri != null)
        {
            highlighter.DrawPolyTriPos(_client.Data, _mouseOverHandler.MouseOverTri.GetPosition());
        }
    }

    private void Tooltip(Vector2 mapPos)
    {
        if (_mouseOverHandler.MouseOverWaypoint != null)
        {
            var template = new TacWaypointTooltipTemplate();
            _client.GetComponent<TooltipManager>()
                .PromptTooltip(template, _mouseOverHandler.MouseOverWaypoint);
        }
        
    }
    private void TryOpenRegimeOverview()
    {
        var poly = _mouseOverHandler.MouseOverPoly;
        if (poly == null)
        {
            throw new Exception();
        }
        if (poly.OwnerRegime.Fulfilled())
        {
            var r = poly.OwnerRegime.Entity(_client.Data);
            var w = Game.I.Client.GetComponent<WindowManager>().OpenWindow<RegimeOverviewWindow>();
            w.Setup(r, _client);
        }
    }

    private void PaintOccupation(Vector2 mapPos)
    {
        var found = _client.Data.Military.WaypointGrid.TryGetClosest(mapPos, out var close, wp => true);
        if (found)
        {
            var regime = _client.Data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(_client.Data);

            _client.Data.Military.TacticalWaypoints.OccupierRegimes[close.Id] = regime.Id;
            foreach (var n in close.TacNeighbors(_client.Data))
            {
                if (n is IWaterWaypoint) continue;
                _client.Data.Military.TacticalWaypoints.OccupierRegimes[n.Id] = regime.Id;
            }
        }
    }
}