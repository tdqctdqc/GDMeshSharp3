
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TheaterAssignment : ForceAssignment, ICompoundForceAssignment
{
    public HashSet<ForceAssignment> Assignments { get; private set; }
    public HashSet<int> HeldCellIds { get; private set; }
    public TheaterAssignment(int id, EntityRef<Regime> regime, 
        HashSet<ForceAssignment> assignments,
        HashSet<int> heldCellIds, HashSet<int> groupIds) 
        : base(groupIds, regime, id)
    {
        Assignments = assignments;
        HeldCellIds = heldCellIds;
    }
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        foreach (var fa in Assignments)
        {
            fa.CalculateOrders(orders, key);
        }
    }

    public override float GetPowerPointNeed(Data d)
    {
        return Assignments.Sum(fa => fa.GetPowerPointNeed(d));
    }

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
    public static void PutGroupsInRightTheater(Regime r, 
        IEnumerable<ForceAssignment> forceAssignments, 
        LogicWriteKey key)
    {
        var groups = key.Data.Military.UnitAux.UnitGroupByRegime[r];
        var freeGroups = groups
            .Where(g => forceAssignments.Any(fa => fa.GroupIds.Contains(fa.Id) == false))
            .ToHashSet();
        var claimedGroups = groups.Except(freeGroups).ToHashSet();
        foreach (var freeGroup in freeGroups)
        {
            var freeGroupCells = freeGroup.Units.Items(key.Data)
                .Select(u => u.Position.GetCell(key.Data)).ToHashSet();
            var theater = forceAssignments
                .OfType<TheaterAssignment>()
                .FirstOrDefault(t => freeGroupCells.Any(wp => t.HeldCellIds.Contains(wp.Id)));
            if (theater == null)
            {
                GD.Print($"couldnt find theater for free unit group " +
                         $"of {r.Name} " +
                         $"at wp {freeGroupCells.First().Id}");
                continue;
            }
            theater.GroupIds.Add(freeGroup.Id);
        }
        
        foreach (var claimedGroup in claimedGroups)
        {
            var claimedGroupCells = claimedGroup.Units.Items(key.Data)
                .Select(u => u.Position.GetCell(key.Data)).ToHashSet();
            var theatersClaiming = forceAssignments.OfType<TheaterAssignment>()
                .Where(t => t.GroupIds.Contains(claimedGroup.Id));
            if (theatersClaiming.Count() > 1)
            {
                GD.Print("multiple theaters claiming unit group");
                throw new Exception();
            }
            var theaterClaiming = theatersClaiming.First();
            if (claimedGroupCells.Any(wp => theaterClaiming.HeldCellIds.Contains(wp.Id)) == false)
            {
                GD.Print("theater doesnt have wp for claimed unit group");
                var theatersSharingWp = forceAssignments
                    .OfType<TheaterAssignment>()
                    .Where(ta => claimedGroupCells.Any(wp => ta.HeldCellIds.Contains(wp.Id)));
                if (theatersSharingWp.Count() == 0)
                {
                    GD.Print("couldnt find theater for claimed unit group");
                    continue;
                }
                theaterClaiming.TakeAwayGroup(claimedGroup, key);
                theatersSharingWp.First().GroupIds.Add(claimedGroup.Id);
            }
        }
    }
    public void SetTargets(LogicWriteKey key)
    {
        foreach (var front in Assignments.OfType<FrontAssignment>())
        {
            front.SetTargets(key);
        }
    }
    
    public static void CheckFronts(Regime r, 
        List<TheaterAssignment> theaters, 
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        
        foreach (var ta in theaters)
        {
            var fronts = ta.Assignments.OfType<FrontAssignment>().ToArray();
            foreach (var fa in fronts)
            {
                ta.Assignments.Remove(fa);
            }
            var newFronts = fronts.Blob(ta, key);
            ta.Assignments.AddRange(newFronts);
            foreach (var fa in ta.Assignments.OfType<FrontAssignment>())
            {
                fa.CheckSegments(key);
            }
        }
    }
    public override void AssignGroups(LogicWriteKey key)
    {
        this.AssignFreeGroups(key);
        this.ShiftGroups(key);
        foreach (var fa in Assignments)
        {
            fa.AssignGroups(key);
        }
    }

    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        UnitGroup deassign = deassign = Assignments
            .MaxBy(s => s.GetSatisfiedRatio(key.Data))
            .RequestGroup(key);
        
        if(deassign == null)
        {
            deassign = key.Data.Get<UnitGroup>(GroupIds.First());
        }
        
        if(deassign != null) GroupIds.Remove(deassign.Id);
        return deassign;
    }
    
    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        this.TakeAwayGroupCompound(g, key);
    }
    
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d)
            .FirstOrDefault(wp => wp.Controller.RefId == Regime.RefId);
    }
}