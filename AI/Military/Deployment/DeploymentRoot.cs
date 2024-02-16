
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentBranch
{
    public DeploymentRoot(DeploymentAi ai,
        LogicWriteKey key) : base(ai.Regime, key)
    {
    }
    
    public void MakeTheaters(RegimeMilitaryAi ai, LogicWriteKey key)
    {
        foreach (var theater in ai.Strategic.Theaters)
        {
            var theaterBranch = new TheaterBranch(Regime, theater, key);
            SubBranches.Add(theaterBranch);
            theaterBranch.MakeFronts(ai, key);
        }
    }

    public void GrabUnassignedGroups(LogicWriteKey key)
    {
        var ai = key.Data.HostLogicData.RegimeAis[Regime]
            .Military.Deployment;
        var freeGroups = key.Data.Military.UnitAux.UnitGroupByRegime[Regime];
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
        return Regime.Capital.Entity(d).GetCells(d).First();
    }
    

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Regime.GetPolys(d);
        return d.Planet.GetAveragePosition(polys.Select(p => p.Center));
    }
}