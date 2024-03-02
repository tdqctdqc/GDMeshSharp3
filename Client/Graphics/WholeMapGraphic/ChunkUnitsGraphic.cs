
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ChunkUnitsGraphic : Node2D
{
    public MapChunk Chunk { get; private set; }
    public MeshInstance2D Child { get; private set; }
    public Dictionary<Cell, List<Unit>> UnitsInOrder { get; private set; }
    private ChunkUnitsGraphic() { }
    public ChunkUnitsGraphic(MapChunk chunk, 
        GraphicsSegmenter segmenter, Data d)
    {
        UnitsInOrder = new Dictionary<Cell, List<Unit>>();
        Chunk = chunk;
        segmenter.AddElement(this, Chunk.RelTo.Center);
    }

    public void Draw(Data data, 
        UnitGraphicLayer layer,
        GraphicsSegmenter segmenter)
    {
        if (Child != null)
        {
            Child.QueueFree();
            Child = null;
        }
        PutUnitsInOrder(data);
        DrawUnitsInOrder(data, layer, segmenter);
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
    private void DrawUnitsInOrder(Data data, 
        UnitGraphicLayer layer, GraphicsSegmenter segmenter)
    {
        if (GetParent() == null)
        {
            segmenter.AddElement(this, Chunk.RelTo.Center);
        }
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
                var graphic = layer.GetUnitGraphic(unit, data);
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

    public void CycleUnits(Cell cell, 
        GraphicsSegmenter segmenter,
        UnitGraphicLayer layer, 
        Data d)
    {
        if (UnitsInOrder.ContainsKey(cell) == false) return;
        var list = UnitsInOrder[cell];
        var first = list.First();
        list.Remove(first);
        list.Add(first);
        DrawUnitsInOrder(d, layer, segmenter);
    }
    public void Update(Data data, 
        UnitGraphicLayer layer, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
        queue.Enqueue(() =>
        {
            Draw(data, layer, segmenter);
        });
    }
    
    public override void _Process(double delta)
    {
        var zoom = Game.I.Client.Cam().ScaledZoomOut;
        if (zoom > .5f)
        {
            Visible = false;
        }
        else Visible = true;
    }
}