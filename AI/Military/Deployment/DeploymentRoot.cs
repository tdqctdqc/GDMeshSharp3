
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentBranch
{
    
    public DeploymentRoot(ERef<Alliance> alliance, int id, HashSet<DeploymentBranch> subBranches, HashSet<GroupAssignment> assignments) : base(alliance, id, subBranches, assignments)
    {
    }

    public void MakeTheaters(AllianceMilitaryAi ai, LogicKey key)
    {
        foreach (var theater in ai.Strategic.Theaters.Entities(key.Data))
        {
            var theaterBranch = new TheaterBranch(Alliance, 
                key.Data.IdDispenser.TakeId(),
                new HashSet<DeploymentBranch>(),
                new HashSet<GroupAssignment>(),
                theater.MakeRef());
            SubBranches.Add(theaterBranch);
            theaterBranch.MakeFronts(ai, key);
        }
    }

    public void GrabUnassignedGroups(LogicKey key)
    {
        var alliance = Alliance.Get(key.Data);
        var ai = key.Data.HostLogicData.AllianceAis[alliance]
            .Military.Deployment;

        var freeGroups =
            key.Data.GetAll<Army>()
                .Where(g => alliance.Members.Contains(g.Regime))
            .ToHashSet();
        if (freeGroups.Count == 0) return;
        var taken = GetDescendentAssignments()
            .SelectMany(a => a.Groups);
        freeGroups.ExceptWith(taken.Select(t => t.Get(key.Data)));
        var byCell = freeGroups.SortBy(g => g.GetHomeCell(key.Data));
        foreach (var (cell, groups) in byCell)
        {
            var unassigned = new UnoccupiedAssignment(
                key.Data.IdDispenser.TakeId(),
                this,
                alliance.MakeRef(),
                new HashSet<ERef<Army>>(),
                cell.MakeRef());
            Assignments.Add(unassigned);
            foreach (var g in groups)
            {
               unassigned.PushGroup(ai, g, key);
            }
        }
    }
    
    public override Cell GetCharacteristicCell(Data d)
    {
        return Alliance.Get(d).Leader.Get(d).Capital.Get(d);
    }
    

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Alliance.Get(d).Members.Entities(d)
            .SelectMany(r => r.GetCells(d));
        return d.Planet.GetAveragePosition(polys.Select(p => p.GetCenter()));
    }
}