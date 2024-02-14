
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentBranch
{
    public DeploymentRoot(DeploymentAi ai,
        LogicWriteKey key) : base(ai, key)
    {
    }
    
    public void MakeTheaters(DeploymentAi ai, LogicWriteKey key)
    {
        var cells = key.Data.Planet.PolygonAux
            .PolyCells.Cells.Values
            .Where(c => c.Controller.RefId == Regime.RefId)
            .ToArray();
        var unions = UnionFind.Find(cells,
            (p, q) => true, p => p.GetNeighbors(key.Data));
        var newTheaters = unions.Select(u =>
            new Theater(ai, u.ToHashSet(), key));
        foreach (var theater in newTheaters)
        {
            SubBranches.Add(theater);
            theater.MakeFronts(ai, key);
        }
    }

    public void GrabUnassignedGroups(LogicWriteKey key)
    {
        var ai = key.Data.HostLogicData.RegimeAis[Regime.Entity(key.Data)]
            .Military.Deployment;
        var freeGroups = key.Data.Military.UnitAux.UnitGroupByRegime[Regime.Entity(key.Data)];
        var taken = GetDescendentAssignments()
            .SelectMany(a => a.Groups);
        freeGroups.ExceptWith(taken);
        var byCell = freeGroups.SortInto(g => g.GetCell(key.Data));
       foreach (var (cell, groups) in byCell)
       {
           var unassigned = new UnoccupiedAssignment(cell, this, ai, key);
           Assignments.Add(unassigned);
           foreach (var g in groups)
           {
               unassigned.PushGroup(ai, g, key);
           }
       }
    }
    

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return Regime.Entity(d).Capital.Entity(d).GetCells(d).First();
    }
    

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Regime.Entity(d).GetPolys(d);
        return d.Planet.GetAveragePosition(polys.Select(p => p.Center));
    }
}