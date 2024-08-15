
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentRoot : DeploymentBranch
{
    
    public DeploymentRoot(ERef<Regime> regime, int id, HashSet<DeploymentBranch> subBranches, HashSet<ArmyAssignment> assignments) : base(regime, id, subBranches, assignments)
    {
    }

    public void MakeTheaters(RegimeMilitaryAi ai, LogicKey key)
    {
        var theaters = key.Data.GetAll<Theater>()
            .Where(t => t.Regime.Equals(Regime))
            .ToArray();
        foreach (var theater in theaters)
        {
            var theaterBranch = new TheaterBranch(Regime, 
                key.Data.IdDispenser.TakeId(),
                new HashSet<DeploymentBranch>(),
                new HashSet<ArmyAssignment>(),
                theater.MakeRef());
            var theaterCells = theater.Cells.Select(c => c.Get(key.Data));
            var avgPos = key.Data.Planet.GetAveragePosition(theaterCells.Select(c => c.GetCenter()));
            var centerCell = theaterCells.MinBy(c => c.GetCenter().Offset(avgPos, key.Data));
            var theaterReserve = new ReserveAssignment(theaterBranch,
                Regime, new HashSet<ERef<Army>>(), centerCell.MakeRef(),
                theaterBranch.Theater, key.Data.IdDispenser.TakeId());
            theaterBranch.Assignments.Add(theaterReserve);
            SubBranches.Add(theaterBranch);
            theaterBranch.MakeFrontAssignments(ai, key);
        }
    }

    // public void GrabUnassignedGroups(LogicKey key)
    // {
    //     var alliance = Alliance.Get(key.Data);
    //     var ai = key.Data.HostLogicData.AllianceAis[alliance]
    //         .Military.Deployment;
    //
    //     var free =
    //         key.Data.GetAll<Army>()
    //             .Where(g => alliance.Members.Contains(g.Regime))
    //         .ToHashSet();
    //     if (free.Count == 0) return;
    //     var taken = GetDescendentAssignments()
    //         .SelectMany(a => a.Armies);
    //     free.ExceptWith(taken.Select(t => t.Get(key.Data)));
    //     var byCell = free.SortBy(g => g.GetHomeCell(key.Data));
    //     foreach (var (cell, groups) in byCell)
    //     {
    //         var unassigned = new UnoccupiedAssignment(
    //             key.Data.IdDispenser.TakeId(),
    //             this,
    //             alliance.MakeRef(),
    //             new HashSet<ERef<Army>>(),
    //             cell.MakeRef());
    //         Assignments.Add(unassigned);
    //         foreach (var g in groups)
    //         {
    //            unassigned.PushUnit(g, key);
    //         }
    //     }
    // }
    
    public override Cell GetCharacteristicCell(Data d)
    {
        return Regime.Get(d).Capital.Get(d);
    }

    public override void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        var ns = GetDescendentNodes();
        foreach (var n in ns)
        {
            n.Draw(mb, relTo, d);
        }
    }


    public override Vector2 GetMapPosForDisplay(Data d)
    {
        var polys = Regime.Get(d).GetCells(d);
        return d.Planet.GetAveragePosition(polys.Select(p => p.GetCenter()));
    }
}