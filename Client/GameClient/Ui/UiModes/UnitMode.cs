
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UnitMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public UnitMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedCell += c => Highlight();
        _mouseOverHandler.ChangedPoly += c => Highlight();
    }
    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }
    
    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        mapPos = mapPos.ClampPosition(_client.Data);

        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }
        if (e is InputEventMouseButton m && m.ButtonIndex == MouseButton.Left && m.Pressed == false)
        {
            CycleUnits();
            Highlight();
        }
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

    private void Highlight()
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
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        highlight.Clear();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null
            || cell.GetUnits(_client.Data) is HashSet<Unit> units == false
            || units.Count == 0)
        {
            return;
        }
        _client.HighlightPoly(_mouseOverHandler.MouseOverPoly);
        _client.HighlightCell(_mouseOverHandler.MouseOverCell);
        var unitGraphics = _client.GetComponent<MapGraphics>()
            .GraphicLayerHolder.Layers.OfType<UnitGraphicLayer>().First();
        var unit = unitGraphics
            .Graphics[cell.GetChunk(_client.Data)]
            .UnitsInOrder[cell].First();
        var group = unit.GetGroup(_client.Data);
        highlight.Draw(mb => mb.DrawMovementRecord(unit.Id, 4, cell.GetCenter(), _client.Data),
            cell.GetCenter());
        if (group != null && group.GroupOrder != null)
        {
            highlight.Draw(mb => group.GroupOrder.Draw(group, cell.GetCenter(), mb, _client.Data),
                cell.GetCenter());
        }
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