
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentBranch
{
    public DeploymentRoot(DeploymentAi ai,
        LogicWriteKey key) : base(ai.Alliance, key)
    {
    }
    
    public void MakeTheaters(AllianceMilitaryAi ai, LogicWriteKey key)
    {
        foreach (var theater in ai.Strategic.Theaters)
        {
            var theaterBranch = new TheaterBranch(Alliance, theater, key);
            SubBranches.Add(theaterBranch);
            theaterBranch.MakeFronts(ai, key);
        }
    }

    public void GrabUnassignedGroups(LogicWriteKey key)
    {
        var ai = key.Data.HostLogicData.AllianceAis[Alliance]
            .Military.Deployment;
        var freeGroups =
            Alliance.Members.Items(key.Data)
                .SelectMany(r => key.Data.Military
                    .UnitAux.UnitGroupByRegime[r]).ToHashSet();
            
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
    
    public override Cell GetCharacteristicCell(Data d)
    {
        return Alliance.Leader.Get(d).Capital.Get(d).GetCells(d).First();
    }
    

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Alliance.Members.Items(d)
            .SelectMany(r => r.GetPolys(d));
        return d.Planet.GetAveragePosition(polys.Select(p => p.Center));
    }
}