
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
    public Dictionary<PolyCell, HashSet<Unit>> UnitsByCell { get; private set; }
    public PathCache PathCache { get; private set; }
    public Context(Data data)
    {
        ControlledAreas = new Dictionary<Regime, HashSet<LandCell>>();
        MovementRecords = new ConcurrentDictionary<int, MovementRecord>();
        UnitsByCell = new Dictionary<PolyCell, HashSet<Unit>>();
        PathCache = new PathCache(data);
    }

    public void Calculate(Data data)
    {
        CalculateControlAreas(data);
        CalculateUnitCells(data);
    }

    public void AddToMovementRecord(int id, MapPos pos, Data d)
    {
        var record = MovementRecords.GetOrAdd(id, i => new MovementRecord());
        record.Add((d.BaseDomain.GameClock.Tick, pos.PolyCell));
    }
    private void CalculateControlAreas(Data data)
    {
        ControlledAreas.Clear();
        var landCells = data.Planet.PolygonAux.PolyCells
            .Cells.Values.OfType<LandCell>().ToHashSet();
        var unions = landCells.SortInto(c => c.Controller.Entity(data));
            
        foreach (var union in unions)
        {
            var regime = union.Key;
            ControlledAreas.Add(regime, union.Value.ToHashSet());
        }
    }

    private void CalculateUnitCells(Data d)
    {
        UnitsByCell.Clear();
        foreach (var c in d.Planet.PolygonAux.PolyCells.Cells.Values)
        {
            UnitsByCell.Add(c, new HashSet<Unit>());
        }
        foreach (var unit in d.GetAll<Unit>())
        {
            var cell = unit.Position.GetCell(d);
            UnitsByCell[cell].Add(unit);
        }
    }
}