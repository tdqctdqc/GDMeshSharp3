
using System;
using System.Linq;
using Godot;

public class UnitMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public UnitMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler();
    }
    public override void Process(float delta)
    {
    }
    
    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        mapPos = mapPos.ClampPosition(_client.Data);
        Game.I.Client.Cam().HandleInput(e);
        _mouseOverHandler.Process(_client.Data, mapPos);

        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }
        Highlight(mapPos);
        var debug = _client.GetComponent<MapGraphics>().DebugOverlay;
        debug.Clear();
        var u = GetCloseUnit(mapPos);
        if(u != null)
        {
            UnitTooltip(u);
            OverlayForUnit(u);
        }
        else
        {
            _client.GetComponent<MapGraphics>().DebugOverlay.Clear();
        }
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
    }

    private void Highlight(Vector2 mapPos)
    {
        _client.HighlightPoly(_mouseOverHandler.MouseOverPoly);
    }

    private Unit GetCloseUnit(Vector2 mapPos)
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null) return null;

        var units = _client.Data.Context
            .UnitsByCell[cell];
        return units.FirstOrDefault();
    }
    private void UnitTooltip(Unit close)
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
        var tooltip = _client.GetComponent<TooltipManager>();
        if (close != null)
        {
            tooltip.PromptTooltip(new UnitTooltipTemplate(), close);
        }
        else
        {
            tooltip.Clear();
        }
    }

    private void OverlayForUnit(Unit u)
    {
        
    }
    
}