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
            new HashSet<int>(),
            r,
            attack,
            ColorsExt.GetRandomColor());
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<(int nativeId, int foreignId)> frontLineFaces,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        bool attack,
        Color color) 
        : base(groupIds, regime, id)
    {
        Color = color;
        if (frontLineFaces.Count == 0) throw new Exception();
        Attack = attack;
        FrontLineFaces = frontLineFaces;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders,
        LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        var cells = GetCells(key.Data).ToList();
        var rear = GetRear(key.Data.Models.MoveTypes.InfantryMove,
            3, key.Data);
        var groups = Groups(key.Data).ToList();
        var readyGroups = GetReadyGroups(key.Data);
        
        var unreadyGroups = groups
            .Except(readyGroups).ToHashSet();
        if (readyGroups.Count() > 0)
        {
            var assgns =
                Assigner.PickBestAndAssignAlongFaces<UnitGroup, (int nativeId, int foreignId)>(
                    FrontLineFaces,
                    readyGroups.ToList(),
                    g => g.GetPowerPoints(key.Data),
                    (g, f) => 
                        g.GetCell(key.Data).GetCenter().GetOffsetTo(
                            PlanetDomainExt.GetPolyCell(f.nativeId, key.Data).GetCenter(), key.Data).Length(),
                    readyGroups.Sum(g => g.GetPowerPoints(key.Data)),
                    f =>
                    {
                        var foreignCell = PlanetDomainExt.GetPolyCell(f.foreignId, key.Data);
                        if (foreignCell.Controller.RefId == -1) return 0f;
                        var foreignRegime = foreignCell.Controller.Entity(key.Data);
                        var foreignAlliance = foreignRegime.GetAlliance(key.Data);

                        var units = foreignCell.GetUnits(key.Data);
                        if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;

                        if (alliance.Rivals.Contains(foreignAlliance) == false) return 0f;
                        float mult = 1f;
                        if (alliance.AtWar.Contains(foreignAlliance)) mult = 2f;
                        return units.Sum(u => u.GetPowerPoints(key.Data)) * mult;
                    }
                );
                
                
                
            foreach (var readyGroup in readyGroups)
            {
                if (assgns.ContainsKey(readyGroup) == false)
                {
                    unreadyGroups.Add(readyGroup);
                    continue;
                }
                var assgn = assgns[readyGroup];
                var order = new DeployOnLineGroupOrder(
                    assgn, 
                    Attack
                );
                var proc = new SetUnitOrderProcedure(readyGroup.MakeRef(), order);
                key.SendMessage(proc);
            }
        }
        
        foreach (var unreadyGroup in unreadyGroups)
        {
            var moveType = unreadyGroup.MoveType(key.Data);
            var dest = PathFinder<PolyCell>
                .FindClosest(
                    unreadyGroup.GetCell(key.Data),
                    rear.Contains,
                    c => c.GetNeighbors(key.Data).Where(n => moveType.Passable(n, alliance, key.Data)),
                    (p, q) => unreadyGroup.MoveType(key.Data).EdgeCost(p, q, key.Data));
            var order = GoToCellGroupOrder.Construct(
                dest, 
                unreadyGroup.Regime.Entity(key.Data),
                unreadyGroup, key.Data);
            if (order != null)
            {
                var proc = new SetUnitOrderProcedure(unreadyGroup.MakeRef(), order);
                key.SendMessage(proc);
            }
        }
    }

    public HashSet<UnitGroup> GetReadyGroups(Data d)
    {
        var moveType = d.Models.MoveTypes.InfantryMove;
        var a = Regime.Entity(d).GetAlliance(d);
        var closeDist = 500f;
        var closeWps = GetRear(moveType, 3, d)
            .Union(GetCells(d))
            .Where(wp => moveType.Passable(wp, a, d))
            .ToHashSet();
        var readyGroups = Groups(d)
            .Where(g =>closeWps.Contains(g.GetCell(d)));
        return readyGroups.ToHashSet();
    }
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return FrontLineFaces
            .Select(face => PlanetDomainExt.GetPolyCell(face.nativeId, d));
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
        if (GroupIds.Count < 2) return null;
        int de = GroupIds.First();
        GroupIds.Remove(de);
        return key.Data.Get<UnitGroup>(de);
    }

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        if(GroupIds.Count == 1)
        GroupIds.Remove(g.Id);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
    }

    public int GetLength(Data d)
    {
        return FrontLineFaces.Count;
    }

    public HashSet<PolyCell> GetRear(MoveType moveType, int radius, Data d)
    {
        var cells = GetCells(d).ToHashSet();
        var alliance = Regime.Entity(d).GetAlliance(d);
       
        for (var i = 0; i < radius; i++)
        {
            var ns = cells
                .SelectMany(c => c.GetNeighbors(d))
                .Where(n => n.Controlled(alliance, d)
                    && moveType.Passable(n, alliance, d))
                .ToArray();
            cells.AddRange(ns);
        }

        return cells;
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d)
            .FirstOrDefault(wp => wp.Controller.RefId == Regime.RefId);
    }
}