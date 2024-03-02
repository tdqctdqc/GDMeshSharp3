
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public class UnitGraphicLayer : GraphicLayer<MapChunk, ChunkUnitsGraphic>
{
    private Dictionary<Unit, UnitGraphic> _unitGraphics;
    public UnitGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base(LayerOrder.Units, "Units", segmenter)
    {
        _unitGraphics = new Dictionary<Unit, UnitGraphic>();
        foreach (var unit in d.GetAll<Unit>())
        {
            var unitGraphic = new UnitGraphic(unit, d);
            _unitGraphics.Add(unit, unitGraphic);
        }
        d.SubscribeForCreation<Unit>(u =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                var unitGraphic = new UnitGraphic((Unit)u.Entity, d);
                _unitGraphics.Add((Unit)u.Entity, unitGraphic);
            });
        });
        d.SubscribeForDestruction<Unit>(u =>
        {
            var unit = (Unit)u.Entity;
            var graphic = _unitGraphics[unit];
            graphic.QueueFree();
            _unitGraphics.Remove(unit);
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
            client.QueuedUpdates.Enqueue(() =>
            {
                foreach (var (unit, graphic) in _unitGraphics)
                {
                    graphic.Draw(unit, d);
                }
            });
        });
    }

    public void CycleCell(Cell cell, Data d)
    {
        var chunk = cell.GetChunk(d);
        var graphic = Graphics[chunk];
        graphic.CycleUnits(cell, _segmenter, this, d);
    }

    public UnitGraphic GetUnitGraphic(Unit u, Data d)
    {
        return _unitGraphics.GetOrAdd(u, u => new UnitGraphic(u, d));
    }
    protected override ChunkUnitsGraphic GetGraphic(MapChunk key, Data d)
    {
        var g = new ChunkUnitsGraphic(key, _segmenter, d);
        g.Draw(d, this, _segmenter);
        return g;
    }
}