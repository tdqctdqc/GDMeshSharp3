
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public class UnitGraphicLayer : GraphicLayer<MapChunk, ChunkUnitsGraphic>
{
    public Dictionary<Unit, UnitGraphic> UnitGraphics { get; private set; }
    public UnitGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base(LayerOrder.Units, "Units", segmenter)
    {
        UnitGraphics = new Dictionary<Unit, UnitGraphic>();
        foreach (var unit in d.GetAll<Unit>())
        {
            var unitGraphic = new UnitGraphic();
            UnitGraphics.Add(unit, unitGraphic);
        }
        d.SubscribeForCreation<Unit>(u =>
        {
            var unitGraphic = new UnitGraphic();
            UnitGraphics.Add((Unit)u.Entity, unitGraphic);
        });
        d.SubscribeForDestruction<Unit>(u =>
        {
            var unit = (Unit)u.Entity;
            var graphic = UnitGraphics[unit];
            graphic.QueueFree();
            UnitGraphics.Remove(unit);
        });
        
        foreach (var cell in d.Planet.PolygonAux.Chunks)
        {
            Add(cell, d);
        }

        client.Data.Notices.Ticked.Blank.Subscribe(() =>
        {
            foreach (var g in Graphics)
            {
                g.Value.Update(d, this, segmenter, client.QueuedUpdates);
            }
        });
    }

    public void CycleCell(PolyCell cell, Data d)
    {
        var chunk = cell.GetChunk(d);
        var graphic = Graphics[chunk];
        graphic.CycleUnits(cell, _segmenter, this, d);
    }
    protected override ChunkUnitsGraphic GetGraphic(MapChunk key, Data d)
    {
        var g = new ChunkUnitsGraphic(key, _segmenter, d);
        g.Draw(d, this, _segmenter);
        return g;
    }
}