using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapInputCatcher : Node
{
    private MouseOverPolyHandler _mouseOverHandler;
    private Data _data;
    public MapInputCatcher(Data data)
    {
        _data = data;
        _mouseOverHandler = new MouseOverPolyHandler();
    }

    private MapInputCatcher()
    {
    }
    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            var mapPos = Game.I.Client.Cam().GetMousePosInMapSpace();
            var d = GetProcessDeltaTime();
            _mouseOverHandler.Process((float) d, _data, mapPos);
        }
        if(e.IsAction("Open Regime Overview"))
        {
            TryOpenRegimeOverview();
        }

        Game.I.Client.Cam().Process(e);
    }


    private void TryOpenRegimeOverview()
    {
        var poly = _mouseOverHandler.MouseOverPoly;
        if (poly == null) return;
        if (poly.Regime.Fulfilled())
        {
            var r = poly.Regime.Entity(_data);
            var w = Game.I.Client.GetComponent<WindowManager>().OpenWindow<RegimeOverviewWindow>();
            w.Setup(r, _data);
        }
    }
}
