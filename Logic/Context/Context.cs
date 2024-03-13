
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Godot;

public class Context
{
    public ConcurrentDictionary<int, MovementRecord> MovementRecords { get; private set; }
    public Dictionary<Regime, HashSet<LandCell>> ControlledAreas { get; private set; }
    public PathCache PathCache { get; private set; }
    public Context(Data data)
    {
        ControlledAreas = new Dictionary<Regime, HashSet<LandCell>>();
        MovementRecords = new ConcurrentDictionary<int, MovementRecord>();
        PathCache = new PathCache(data);
        data.Notices.Ticked.Subscribe(i => PathCache.Clear());
    }

    public void Calculate(Data data)
    {
        CalculateControlAreas(data);
    }

    public void AddToMovementRecord(int id, MapPos pos, Data d)
    {
        var record = MovementRecords.GetOrAdd(id, i => new MovementRecord());
        record.Add((d.BaseDomain.GameClock.Tick, pos.PolyCell));
    }
    private void CalculateControlAreas(Data data)
    {
        ControlledAreas.Clear();
        var landCells = data.Planet.MapAux.CellHolder
            .Cells.Values.OfType<LandCell>().ToHashSet();
        var unions = landCells.SortInto(c => c.Controller.Get(data));
            
        foreach (var union in unions)
        {
            var regime = union.Key;
            ControlledAreas.Add(regime, union.Value.ToHashSet());
        }
    }

}