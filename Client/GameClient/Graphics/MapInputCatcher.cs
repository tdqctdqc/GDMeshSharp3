using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapInputCatcher : Node
{
    private MapGraphics _graphics;
    private ClientWriteKey _key;
    private MouseOverPolyHandler _mouseOverHandler;
    public MapInputCatcher(ClientWriteKey key, MapGraphics graphics)
    {
        _key = key;
        _graphics = graphics;
        _mouseOverHandler = new MouseOverPolyHandler();
    }

    private MapInputCatcher()
    {
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            var mapPos = Game.I.Client.Cam.GetMousePosInMapSpace();
            var d = GetProcessDeltaTime();
            _mouseOverHandler.Process((float) d, _key.Data, mapPos);
            // GetViewport().SetInputAsHandled();
        }

        if (e is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Right)
            {
                TryOpenRegimeOverview();
            }
        }

        Game.I.Client.Cam.Process(e);
    }


    private void TryOpenRegimeOverview()
    {
        var poly = _mouseOverHandler.MouseOverPoly;
        if (poly.Regime.Fulfilled())
        {
            var r = poly.Regime.Entity();
            var w = Game.I.Client.Requests.OpenWindow<RegimeOverviewWindow>();
            w.Setup(r, _key);
        }
    }
}
