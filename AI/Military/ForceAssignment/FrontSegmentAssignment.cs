using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : ForceAssignment
{
    public static int IdealSegmentLength = 5;
    public List<(int nativeId, int foreignId)> FrontLineFaces { get; private set; }
    public List<int> FaceCoveringGroupIds { get; private set; }
    public Color Color { get; private set; }
    public bool Attack { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<(PolyCell native, PolyCell foreign)> frontLine,
        bool attack,
        LogicWriteKey key)
    {
        var frontLineFaceIds = frontLine.Select(wp => (wp.native.Id, wp.foreign.Id)).ToList();
        var cells = frontLine.Select(face => face.native).ToHashSet();
        var fsa = new FrontSegmentAssignment(
            key.Data.IdDispenser.TakeId(),
            frontLineFaceIds,
            Enumerable.Range(0, frontLineFaceIds.Count)
                .Select(i => -1).ToList(),
            new HashSet<int>(),
            r,
            attack,
            ColorsExt.GetRandomColor());
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<(int nativeId, int foreignId)> frontLineFaces,
        List<int> faceCoveringGroupIds,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        bool attack,
        Color color) 
        : base(groupIds, regime, id)
    {
        Color = color;
        if (frontLineFaces.Count == 0) throw new Exception();
        FaceCoveringGroupIds = faceCoveringGroupIds;
        Attack = attack;
        FrontLineFaces = frontLineFaces;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders,
        LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        

        if (FaceCoveringGroupIds.Any(id => id == -1))
        {
            DoNewGroupToFaceAssignment(key.Data);
        }
        else
        {
            AdjustFaceGroups(key.Data);
        }


        var group = -1;
        var from = -1;
        for (var i = 0; i < FaceCoveringGroupIds.Count; i++)
        {
            var currGroup = FaceCoveringGroupIds[i];
            if (currGroup != group)
            {
                giveLineOrder(group, from, i - 1);
                group = currGroup;
                from = i;
            }
            if (i == FaceCoveringGroupIds.Count - 1)
            {
                giveLineOrder(group, from, i);
            }
        }

        void giveLineOrder(int groupId, int fromIndex, int toIndex)
        {
            if (groupId == -1) return;
            var line = FrontLineFaces.GetRange(fromIndex, toIndex - fromIndex + 1);
            var order = new DeployOnLineGroupOrder(line, false);
            var proc = new SetUnitOrderProcedure(new EntityRef<UnitGroup>(groupId),
                order);
            key.SendMessage(proc);
        }

        var lineGroups = FaceCoveringGroupIds
            .ToHashSet();
        var unoccupiedGroups = GroupIds
            .Except(lineGroups)
            .Select(id => key.Data.Get<UnitGroup>(id));
        
        var frontlineCells = GetCells(key.Data).ToHashSet();
        
        foreach (var unreadyGroup in unoccupiedGroups)
        {
            var moveType = unreadyGroup.MoveType(key.Data);
            var dest = PathFinder<PolyCell>
                .FindClosest(
                    unreadyGroup.GetCell(key.Data),
                    frontlineCells.Contains,
                    c => c.GetNeighbors(key.Data).Where(n => moveType.Passable(n, alliance, key.Data)),
                    (p, q) => unreadyGroup.MoveType(key.Data).EdgeCost(p, q, key.Data));
            var order = GoToCellGroupOrder.Construct(
                dest, 
                unreadyGroup.Regime.Entity(key.Data),
                unreadyGroup, key.Data);
            var proc = new SetUnitOrderProcedure(unreadyGroup.MakeRef(), order);
            key.SendMessage(proc);
        }
    }

    private void DoNewGroupToFaceAssignment(Data d)
    {
        var nativeCells = FrontLineFaces
            .Select(f => PlanetDomainExt.GetPolyCell(f.nativeId, d))
            .ToHashSet();
        var readyGroups = GroupIds
            .Select(id => d.Get<UnitGroup>(id))
            .Where(g => nativeCells.Intersect(g.Units.Items(d).Select(u => u.Position.GetCell(d))).Count() > 0);
        if (readyGroups.Count() == 0) return;
        var alliance = Regime.Entity(d).GetAlliance(d);
        
        var assgns =
            Assigner.PickBestAndAssignAlongFaces<UnitGroup, (int nativeId, int foreignId)>(
                FrontLineFaces,
                readyGroups.ToList(),
                g => g.GetPowerPoints(d),
                (g, f) => 
                    g.GetCell(d).GetCenter().GetOffsetTo(
                        PlanetDomainExt.GetPolyCell(f.nativeId, d).GetCenter(), d).Length(),
                readyGroups.Sum(g => g.GetPowerPoints(d)),
                f =>
                {
                    var foreignCell = PlanetDomainExt.GetPolyCell(f.foreignId, d);
                    if (foreignCell.Controller.RefId == -1) return 0f;
                    var foreignRegime = foreignCell.Controller.Entity(d);
                    var foreignAlliance = foreignRegime.GetAlliance(d);

                    var units = foreignCell.GetUnits(d);
                    if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;

                    if (alliance.Rivals.Contains(foreignAlliance) == false) return 0f;
                    float mult = 1f;
                    if (alliance.AtWar.Contains(foreignAlliance)) mult = 2f;
                    return units.Sum(u => u.GetPowerPoints(d)) * mult;
                }
            );
        foreach (var (unitGroup, faces) in assgns)
        {
            var first = FrontLineFaces.IndexOf(faces.First());
            var last = FrontLineFaces.IndexOf(faces.Last());
            if (first > last) throw new Exception();
            for (var i = first; i <= last; i++)
            {
                FaceCoveringGroupIds[i] = unitGroup.Id;
            }
        }

        // if (FaceCoveringGroupIds.Any(f => f == -1))
        // {
        //     throw new Exception();
        // }
    }

    private void AdjustFaceGroups(Data d)
    {
        
    }

    public IEnumerable<FrontSegmentAssignment> Correct(Data d)
    {
        return this.Yield();
    }
    
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return FrontLineFaces
            .Select(face => PlanetDomainExt.GetPolyCell(face.nativeId, d)).Distinct();
    }
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = GetLength(data);

        return opposing * FrontAssignment.CoverOpposingWeight * FrontAssignment.DesiredOpposingPpRatio
               + length * FrontAssignment.CoverLengthWeight * FrontAssignment.PowerPointsPerCellFaceToCover;
    }
    public float GetOpposingPowerPoints(Data data)
    {
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetCells(data)
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(alliance, data))
            .Sum(n =>
            {
                var us = n.GetUnits(data);
                if (us == null) return 0f;
                return us.Sum(u => u.GetPowerPoints(data));
            });
    }
    
    public void MergeInto(FrontSegmentAssignment merging, LogicWriteKey key)
    {
        GroupIds.AddRange(merging.GroupIds);
    }

    
    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        return null;
        if (GroupIds.Count < 2) return null;
        var notInLine = GroupIds
            .Except(FaceCoveringGroupIds);
        if (notInLine.Count() > 0)
        {
            var de = notInLine.First();
            GroupIds.Remove(de);
            return key.Data.Get<UnitGroup>(de);
        }
        else
        {
            return null;
        }
    }

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        for (var i = 0; i < FaceCoveringGroupIds.Count; i++)
        {
            if (FaceCoveringGroupIds[i] == g.Id)
            {
                FaceCoveringGroupIds[i] = -1;
            }
        }
        GroupIds.Remove(g.Id);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
    }

    public int GetLength(Data d)
    {
        return FrontLineFaces.Count;
    }

    
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d).First();
    }
}