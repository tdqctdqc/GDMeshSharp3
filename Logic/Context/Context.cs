
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
    public Context(Data data)
    {
        FriendlyPathCache = new PathCache(false, data);
        RivalPathCache = new PathCache(true, data);
        PowerPoints = new Dictionary<Cell, float>();
        data.Notices.Ticked.Subscribe(i =>
        {
            FriendlyPathCache.Clear();
            RivalPathCache.Clear();
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
    }


}