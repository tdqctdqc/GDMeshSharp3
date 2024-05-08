
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ChunkUnitsGraphic : Node2D, IChunkGraphicModule
{
    public string Name => "Units";
    public MapChunk Chunk { get; private set; }
    public ChunkGraphicModuleVisibility Visibility { get; }
    public MeshInstance2D Child { get; private set; }
    public Dictionary<Cell, List<Unit>> UnitsInOrder { get; private set; }
    public Node2D Node => this;
    private EntityGraphicReservoir<Unit, UnitGraphic> _graphics;
    private ChunkUnitsGraphic() { }
    public ChunkUnitsGraphic(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        EntityGraphicReservoir<Unit, UnitGraphic> graphics,
        Data d)
    {
        _graphics = graphics;
        Visibility = new ChunkGraphicModuleVisibility(zoomVisibilityRange);
        UnitsInOrder = new Dictionary<Cell, List<Unit>>();
        ZIndex = (int)LayerOrder.Units;
        Chunk = chunk;
    }

    public void Draw(Data data)
    {
        PutUnitsInOrder(data);
        PutUnitGraphicsInOrder(data);
        RedrawUnitGraphics(data);
    }

    private void PutUnitsInOrder(Data data)
    {
        UnitsInOrder.Clear();
        var cells = Chunk.Cells;
        foreach (var cell in cells)
        {
            var us = cell.GetUnits(data);
            if (us == null) continue;
            var units = us
                .OrderBy(u =>
                {
                    var g = u.GetGroup(data);
                    return g == null ? -1 : g.Id;
                }).ToList();
            UnitsInOrder.Add(cell, units);
        }
    }
    private void PutUnitGraphicsInOrder(Data data)
    {
        var cells = Chunk.Cells.OrderBy(c => c.GetCenter().Y);

        foreach (var cell in cells)
        {
            var pos = Chunk.RelTo.GetOffsetTo(cell.GetCenter(), data);
            var width = cell.RelBoundary.Max(v => v.X) - cell.RelBoundary.Min(v => v.X);
            width /= 2f;
            var from = pos + Vector2.Left * width * .5f;
            var to = pos + Vector2.Right * width * .5f;
            if (UnitsInOrder.ContainsKey(cell) == false) continue;
            var units = UnitsInOrder[cell];
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var graphic = _graphics.Graphics[unit];
                graphic.GetParent()?.RemoveChild(graphic);
                var proportion = (float)i / units.Count;
                var relPos = from.Lerp(to, proportion);
                graphic.ZIndex = units.Count - i;
                graphic.ZAsRelative = true;
                graphic.Position = relPos;
                AddChild(graphic);
            }
        }
    }

    private void RedrawUnitGraphics(Data d)
    {
        foreach (var (cell, units) in UnitsInOrder)
        {
            foreach (var unit in units)
            {
                var graphic = _graphics.Graphics[unit];
                graphic.Draw(unit, d);
            }
        }
    }

    public void CycleUnits(Cell cell, 
        Data d)
    {
        if (UnitsInOrder.ContainsKey(cell) == false) return;
        var list = UnitsInOrder[cell];
        var first = list.First();
        list.Remove(first);
        list.Add(first);
        PutUnitGraphicsInOrder(d);
    }


    public void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
        
    }

    public Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        
        return settings;
    }

}