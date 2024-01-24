
using System;
using System.Collections.Generic;
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
        var debug = _client.GetComponent<MapGraphics>().DebugOverlay;
        debug.Clear();
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        mapPos = mapPos.ClampPosition(_client.Data);
        Game.I.Client.Cam().HandleInput(e);
        _mouseOverHandler.Process(_client.Data, mapPos);

        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }

        if (e is InputEventMouseButton m && m.ButtonIndex == MouseButton.Left && m.Pressed == false)
        {
            CycleUnits();
        }
        Highlight(mapPos);
        UnitTooltip();
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
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        highlight.Clear();
        _client.HighlightPoly(_mouseOverHandler.MouseOverPoly);
        _client.HighlightCell(_mouseOverHandler.MouseOverCell);
        OverlayForUnit();

    }

    private void UnitTooltip()
    {
        var tooltip = _client.GetComponent<TooltipManager>();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null
            || cell.GetUnits(_client.Data) is HashSet<Unit> units == false
            || units.Count == 0)
        {
            tooltip.Clear();
            return;
        }
        var unitGraphics = _client.GetComponent<MapGraphics>()
            .GraphicLayerHolder.Layers.OfType<UnitGraphicLayer>().First();
        var close = unitGraphics
            .Graphics[cell.GetChunk(_client.Data)]
            .UnitsInOrder[cell].First();
        tooltip.PromptTooltip(new UnitTooltipTemplate(), close);
    }

    private void OverlayForUnit()
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null
            || cell.GetUnits(_client.Data) is HashSet<Unit> units == false
            || units.Count == 0)
        {
            return;
        }
        var unitGraphics = _client.GetComponent<MapGraphics>()
            .GraphicLayerHolder.Layers.OfType<UnitGraphicLayer>().First();
        var close = unitGraphics
            .Graphics[cell.GetChunk(_client.Data)]
            .UnitsInOrder[cell].First();
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        highlight.Draw(mb => mb.DrawMovementRecord(close.Id, 4, cell.GetCenter(), _client.Data),
            cell.GetCenter());
    }

    private void CycleUnits()
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell != null)
        {
            var unitGraphics = _client.GetComponent<MapGraphics>()
                .GraphicLayerHolder.Layers.OfType<UnitGraphicLayer>().First();
            unitGraphics.CycleCell(_mouseOverHandler.MouseOverCell, _client.Data);
        }
    }
    
}