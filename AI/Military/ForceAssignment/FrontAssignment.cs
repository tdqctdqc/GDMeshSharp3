using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;

public class FrontAssignment : ForceAssignment, ICompoundForceAssignment
{
    
    public static float CoverLengthWeight = 1f;
    public static float CoverOpposingWeight = 3f;
    public static float PowerPointsPerLengthToCover = .1f;
    public HashSet<int> HeldCellIds { get; private set; }
    public HashSet<int> TargetAreaCellIds { get; private set; }
    public HashSet<ForceAssignment> Assignments { get; private set; }
    public FrontAssignment(
        int id,
        EntityRef<Regime> regime, 
        HashSet<int> heldCellIds,
        HashSet<int> targetAreaCellIds,
        HashSet<int> groupIds,
        HashSet<ForceAssignment> assignments) 
        : base(groupIds, regime, id)
    {
        HeldCellIds = heldCellIds;
        TargetAreaCellIds = targetAreaCellIds;
        Assignments = assignments;
    }

    public IEnumerable<PolyCell> HeldCells(Data data)
    {
        return HeldCellIds.Select(i => PlanetDomainExt.GetPolyCell(i, data));
    }
    public float GetOpposingPowerPoints(Data data)
    {
        var alliance = Regime.Entity(data).GetAlliance(data);
        return HeldCells(data)
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(alliance, data))
            .Sum(n => data.Context.UnitsByCell[n].Sum(u => u.GetPowerPoints(data)));
    }
    public override void CalculateOrders(MinorTurnOrders orders, 
        LogicWriteKey key)
    {
        foreach (var assgn in Assignments)
        {
            assgn.CalculateOrders(orders, key);
        }
    }
    

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        this.TakeAwayGroupCompound(g, key);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
        
        this.ShiftGroups(key);
        this.AssignFreeGroups(key);
        foreach (var assgn in Assignments)
        {
            assgn.AssignGroups(key);
        }
    }
    
    
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = Assignments.OfType<FrontSegmentAssignment>().Sum(s => s.GetLength(data));
        return opposing * CoverOpposingWeight + length * CoverLengthWeight * PowerPointsPerLengthToCover;
    }
    public static void CheckSplitRemove(Regime r, 
        TheaterAssignment theater,
        List<FrontAssignment> fronts, 
        Action<FrontAssignment> add, Action<FrontAssignment> remove, LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        for (var i = 0; i < fronts.Count; i++)
        {
            var fa = fronts[i];
            fa.HeldCellIds.RemoveWhere(i => theater.HeldCellIds.Contains(i) == false);
            if (fa.HeldCellIds.Count == 0)
            {
                remove(fa);
                continue;
            }
            var flood = FloodFill<PolyCell>.GetFloodFill(
                fa.HeldCells(key.Data).First(),
                wp => fa.HeldCellIds.Contains(wp.Id),
                wp => wp.GetNeighbors(key.Data));
            var unions = UnionFind.Find(fa.HeldCellIds,
                (i, j) => true,
                i => PlanetDomainExt.GetPolyCell(i, key.Data).Neighbors);

            if (unions.Count() != 1)
            {
                remove(fa);
                var newFronts = Divide(fa, key);
                foreach (var newFa in newFronts)
                {
                    add(newFa);
                }
            }
        }
    }
    
    public static IEnumerable<FrontAssignment> Divide(FrontAssignment fa, 
        LogicWriteKey key)
    {
        var r = fa.Regime.Entity(key.Data);
        var unions = UnionFind.Find(fa.HeldCellIds,
            (i, j) => true,
            i => PlanetDomainExt.GetPolyCell(i, key.Data).Neighbors);

        var newFronts =
            unions.Select(
                u => new FrontAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), u.ToHashSet(),
                    new HashSet<int>(),
                    new HashSet<int>(), 
                    new HashSet<ForceAssignment>()));
        foreach (var group in fa.Groups(key.Data))
        {
            var unitCellIds = group.Units.Items(key.Data)
                .Select(u => u.Position.PolyCell).ToList();
            var mostWpsShared = newFronts
                .MaxBy(t => unitCellIds.Where(t.HeldCellIds.Contains).Count());
            if (unitCellIds.Where(mostWpsShared.HeldCellIds.Contains).Count() == 0)
            {
                var pos = group.GetPosition(key.Data);
                var closest = newFronts
                    .MinBy(t => 
                        group.GetPosition(key.Data).GetOffsetTo(
                            key.Data.Planet.GetAveragePosition(t.HeldCellIds.Select(i =>
                                PlanetDomainExt.GetPolyCell(i, key.Data).GetCenter())),
                            key.Data).Length()
                    );
                closest.GroupIds.Add(group.Id);
            }
            else
            {
                mostWpsShared.GroupIds.Add(group.Id);
            }
        }

        return newFronts;
    }
    public List<List<PolyCell>> GetLines(Data d)
    {
        return null;
        // var alliance = Regime.Entity(d).GetAlliance(d);
        // var frontCells = HeldCells(d)
        //     .Where(c => c.Threatened(alliance, d))
        //     .ToHashSet();
        // var potentialEdges = frontCells
        //     .SelectMany(cell =>
        //     {
        //         return cell.Neighbors.Where(nId => nId < cell.Id)
        //             .Where(nId => frontCells.Contains(nId))
        //             .Select(nId => new Vector2I(cell.Id, nId));
        //     })
        //     .ToHashSet();
        // var edges = potentialEdges.ToHashSet();
        // return OrderSegments(edges, d);
        //
    }

    // private List<List<PolyCell>> OrderSegments(
    //     HashSet<Vector2I> edges, Data d)
    // {
    //     var chains = edges
    //         .Select(getLineSeg).ToList()
    //         .GetChains();
    //     var res = new List<List<Vector2>>();
    //     for (var i = 0; i < chains.Count; i++)
    //     {
    //         var chain = chains[i];
    //         if (chain.Count <=
    //             FrontSegmentAssignment.IdealSegmentLength * 1.5f)
    //         {
    //             res.Add(chain.GetPoints());
    //             continue;
    //         }
    //         var numChains = Mathf.FloorToInt((float)chain.Count / FrontSegmentAssignment.IdealSegmentLength);
    //         var numLinksPerChain = Mathf.CeilToInt((float)chain.Count / numChains);
    //         var iter = 0;
    //         var curr = new List<LineSegment>();
    //         for (var j = 0; j < chain.Count; j++)
    //         {
    //             curr.Add(chain[j]);
    //             iter++;
    //             if (iter == numLinksPerChain || j == chain.Count - 1)
    //             {
    //                 res.Add(curr.GetPoints());
    //                 iter = 0;
    //                 curr = new List<LineSegment>();
    //             }
    //         }
    //     }
    //
    //     return res
    //         .Select(c => c.Select(p => d.Military.TacticalWaypoints.ByPos[p])
    //             .Select(id => MilitaryDomain.GetWaypoint(id, d)).ToList())
    //         .ToList();
    //     
    //     LineSegment getLineSeg(Vector2I edge)
    //     {
    //         return new LineSegment(MilitaryDomain.GetWaypoint(edge.X, d).Pos,
    //             MilitaryDomain.GetWaypoint(edge.Y, d).Pos);
    //     }
    // }
    public static void CheckExpandMergeNew(Regime r, 
        TheaterAssignment ta,
        List<FrontAssignment> fronts, 
        Action<FrontAssignment> add,
        Action<FrontAssignment> remove,
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        var frontline = 
            ta.GetCells(key.Data)
            .Where(wp => wp.RivalControlled(alliance, key.Data));
        var unions = UnionFind.Find(frontline, (w, v) => true,
            w => w.GetNeighbors(key.Data));
        var dic = unions.ToDictionary(u => u, u => new List<FrontAssignment>());
        foreach (var fa in fronts)
        {
            var faWps = fa.HeldCells(key.Data);
            var mostShared = unions
                .MaxBy(u => u.Where(wp => fa.HeldCellIds.Contains(wp.Id)).Count());

            if (mostShared.Where(wp => fa.HeldCellIds.Contains(wp.Id)).Count() == 0)
            {
                remove(fa);
            }
            else
            {
                dic[mostShared].Add(fa);
            }
        }
        foreach (var kvp in dic)
        {
            if (kvp.Value.Count() == 0)
            {
                var fa = new FrontAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), kvp.Key.Select(wp => wp.Id).ToHashSet(),
                    new HashSet<int>(),
                    new HashSet<int>(),
                    new HashSet<ForceAssignment>());
                add(fa);
                continue;
            }
            var biggest = kvp.Value.MaxBy(fa => fa.HeldCellIds.Count());
            biggest.HeldCellIds.Clear();
            biggest.HeldCellIds.AddRange(kvp.Key.Select(wp => wp.Id));
            foreach (var fa in kvp.Value)
            {
                if (fa == biggest) continue;
                biggest.MergeInto(fa, key);
                remove(fa);
            }
        }

    }

    public void MergeInto(FrontAssignment dissolve, LogicWriteKey key)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Assignments.AddRange(dissolve.Assignments);
    }


    public void CheckSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d)
            .ToDictionary(l => l, 
                l => new List<FrontSegmentAssignment>());
        
        var segments = Assignments.OfType<FrontSegmentAssignment>().ToList();
        foreach (var fsa in segments)
        {
            var relatedLines = lines.Keys
                .Where(l => l.Any(wp => fsa.FrontLineCellIds.Contains(wp.Id)));
            if (relatedLines.Count() == 0)
            {
                Assignments.Remove(fsa);
                continue; 
            }

            var mostRelated = relatedLines
                .MaxBy(l => l.Where(wp => fsa.FrontLineCellIds.Contains(wp.Id)).Count());
            lines[mostRelated].Add(fsa);
        }
        
        foreach (var kvp in lines)
        {
            var line = kvp.Key;
            var segs = kvp.Value;
            if (segs.Count() == 0)
            {
                var seg = FrontSegmentAssignment
                    .Construct(Regime, line, 
                        null, false, key);
                Assignments.Add(seg);
                continue;
            }

            var biggest = segs.MaxBy(s => s.GroupIds.Count());
            
            biggest.FrontLineCellIds.Clear();
            biggest.FrontLineCellIds.AddRange(line.Select(wp => wp.Id));
            foreach (var fsa in segs)
            {
                if (fsa == biggest) continue;
                biggest.MergeInto(fsa, key);
                Assignments.Remove(fsa);
            }
        }
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return HeldCellIds.Select(i => PlanetDomainExt.GetPolyCell(i, d))
            .FirstOrDefault(wp => wp.Controller.RefId == Regime.RefId);
    }

    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        UnitGroup deassign = null;
        if (Assignments.Count > 0)
        {
            deassign = Assignments
                .MaxBy(s => s.GetSatisfiedRatio(key.Data))
                .RequestGroup(key);
        }
        
        if(deassign == null)
        {
            deassign = key.Data.Get<UnitGroup>(GroupIds.First());
        }
        
        if(deassign != null) GroupIds.Remove(deassign.Id);
        return deassign;
    }

    public void SetTargets(LogicWriteKey key)
    {
        // var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        // var wps = HeldCells(key.Data);
        // var targets = expand(wps);
        // // targets = expand(targets);
        // TargetAreaCellIds.Clear();
        // TargetAreaCellIds.AddRange(targets.Select(t => t.Id));
        // IEnumerable<Waypoint> expand(IEnumerable<Waypoint> source)
        // {
        //     return source
        //         .SelectMany(wp => wp.GetNeighbors(key.Data)
        //             .Where(n => n.IsHostile(alliance, key.Data)));
        // }
        // foreach (var seg in Assignments.OfType<FrontSegmentAssignment>())
        // {
        //     seg.SetAdvance(true, key);
        // }
    }

    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return HeldCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
    }
}