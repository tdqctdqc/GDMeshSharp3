
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UnitMode : UiMode
{
    public DefaultSettingsOption<Unit> Unit { get; private set; }
    private MeshInstance2D _selectedUnitHighlight;
    private MouseOverHandler _mouseOverHandler;
    public UnitMode(Client client) : base(client, "Unit")
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedCell += c => Draw();
        Unit = new DefaultSettingsOption<Unit>("Unit",
            null);
        Unit.SettingChanged.Subscribe(n => Draw());
    }
    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }
    
    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton m 
            && m.ButtonIndex == MouseButton.Left 
            && m.Pressed == false)
        {
            SelectAndCycleUnits();
        }
    }

    public override void Enter()
    {
        var mb = new MeshBuilder();
        mb.AddCircle(Vector2.Zero, 20f, 20, new Color(Colors.Yellow, .5f));
        _selectedUnitHighlight = mb.GetMeshInstance();
        _selectedUnitHighlight.ZIndex = 99;
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
        _selectedUnitHighlight.QueueFree();
    }

    private void Draw()
    {
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        highlight.Clear();
        _mouseOverHandler.Highlight();
        OverlayForUnit();
        UnitTooltip();
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
        var layerHolder = _client.GetComponent<MapGraphics>()
            .GraphicLayerHolder;
        var chunkGraphic = layerHolder.Chunks[cell.GetChunk(_client.Data)];
        var close = chunkGraphic.Units.UnitsInOrder[cell]
            .First();
        tooltip.PromptTooltip(new UnitTooltipTemplate(), close);
    }

    private void OverlayForUnit()
    {
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        
        if (Unit.Value == null)
        {
            return;
        }
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null)
        {
            _selectedUnitHighlight.Visible = false;
            return;
        }
        
        var unitCell = Unit.Value.Position.GetCell(_client.Data);
        _selectedUnitHighlight.Visible = true;
        _client.GetComponent<MapGraphics>().Segmenter
            .AddElement(_selectedUnitHighlight, unitCell.GetCenter());
        
        var layerHolder = _client.GetComponent<MapGraphics>()
            .GraphicLayerHolder;
        var chunkGraphic = layerHolder.Chunks[cell.GetChunk(_client.Data)];
        
        var group = Unit.Value.GetGroup(_client.Data);
        highlight.Draw(mb => mb
                .DrawMovementRecord(Unit.Value.Id,
            4, cell.GetCenter(), _client.Data),
            cell.GetCenter());
        if (group != null && group.GroupOrder != null)
        {
            highlight.Draw(mb => group.GroupOrder.Draw(group, cell.GetCenter(), mb, _client.Data),
                cell.GetCenter());
        }
    }

    private void SelectAndCycleUnits()
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell != null)
        {
            var layerHolder = _client.GetComponent<MapGraphics>()
                .GraphicLayerHolder;
            var chunkGraphic = layerHolder
                .Chunks[cell.GetChunk(_client.Data)].Units;
            chunkGraphic.CycleUnits(_mouseOverHandler.MouseOverCell, 
                _client.Data);
            if (chunkGraphic.UnitsInOrder.ContainsKey(cell) == false)
            {
                Unit.Set(null);
                _selectedUnitHighlight.Visible = false;
                return;
            }
            var unitsInCell = chunkGraphic.UnitsInOrder[cell];
            if (unitsInCell == null 
                || unitsInCell.Count == 0)
            {
                Unit.Set(null);
                _selectedUnitHighlight.Visible = false;
                return;
            }
            Unit.Set(unitsInCell.First());
        }
    }
}