
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Godot;

public class Context
{
    public PathCache FriendlyPathCache { get; private set; }
    public PathCache RivalPathCache { get; private set; }
    public Dictionary<Cell, float> PowerPoints { get; private set; }
    public Dictionary<Army, Cell> ArmyHomeCells { get; private set; }
    public Context(Data data)
    {
        FriendlyPathCache = new PathCache(false, data);
        RivalPathCache = new PathCache(true, data);
        PowerPoints = new Dictionary<Cell, float>();
        ArmyHomeCells = new Dictionary<Army, Cell>();
        data.Notices.Ticked.Subscribe(i =>
        {
            FriendlyPathCache.Clear();
            RivalPathCache.Clear();
        });
        data.SubscribeForCreation<Army>(a =>
        {
            SetArmyHomeCell((Army)a.Entity, data);
        });
    }

    public void Calculate(Data data)
    {
        PowerPoints.Clear();
        foreach (var c in data.Planet.MapAux.CellHolder.Cells.Values)
        {
            PowerPoints.Add(c, 0f);
        }
        foreach (var army in data.GetAll<Army>())
        {
            var pp = army.GetPowerPoints(data);
            foreach (var cell in army.GetCells(data))
            {
                PowerPoints[cell] += pp / army.Cells.Count;
            }
        }
        
        ArmyHomeCells.Clear();
        foreach (var army in data.GetAll<Army>())
        {
            SetArmyHomeCell(army, data);
        }
    }

    private void SetArmyHomeCell(Army army, Data data)
    {
        var cells = army.GetCells(data);

        if (army.Cells.Count == 0)
        {
            var r = army.Regime.Get(data);
            GD.Print($"bad army {army.Id} for {r.Name} moving to {r.Capital.RefId}");
            ArmyHomeCells.Add(army, r.Capital.Get(data));
            return;
        }
        
        var poses = cells.Select(c => c.GetCenter());
        var avgPos = data.Planet.GetAveragePosition(poses);
        var centerCell = cells
            .MinBy(c => c.GetCenter().Offset(avgPos, data).Length());
        ArmyHomeCells.Add(army, centerCell);
    }

}