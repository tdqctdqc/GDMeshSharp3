using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapInputCatcher : Node
{
    private MouseOverPolyHandler _mouseOverHandler;
    private Client _client;
    public MapInputCatcher(Client client)
    {
        _client = client;
        _mouseOverHandler = new MouseOverPolyHandler();
    }

    private MapInputCatcher()
    {
    }
    public void HandleInput(InputEvent e)
    {
        var mapPos = Game.I.Client.Cam().GetMousePosInMapSpace();
        
        if (e is InputEventMouseMotion mm)
        {
            var d = GetProcessDeltaTime();
            _mouseOverHandler.Process((float) d, _client.Data, mapPos);
        }
        if(e.IsAction("Open Regime Overview"))
        {
            TryOpenRegimeOverview();
        }

        if (e is InputEventKey k && k.Keycode == Key.R && k.Pressed == false)
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

        Game.I.Client.Cam().Process(e);
    }

    
    private void TryOpenRegimeOverview()
    {
        var poly = _mouseOverHandler.MouseOverPoly;
        if (poly == null)
        {
            GD.Print("poly null");
            return;
        }
        if (poly.OwnerRegime.Fulfilled())
        {
            var r = poly.OwnerRegime.Entity(_client.Data);
            var w = Game.I.Client.GetComponent<WindowManager>().OpenWindow<RegimeOverviewWindow>();
            w.Setup(r, _client);
        }
    }
}
