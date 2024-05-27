
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MilAiUtil
{
    public static float CoverOpposingWeight { get; private set; }
        = .5f;
    public static float CoverLengthWeight { get; private set; }
        = 1f;
    public static float DesiredOpposingPpRatio { get; private set; }
        = 2f;
    public static float PowerPointsPerCellFaceToCover { get; private set; }
        = 100f;
    
    public static float GetFaceEnemyCost(Alliance alliance, 
        FrontFace f, Data d)
    {
        var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
        if (foreignCell.Controller.RefId == -1)
        {
            return 0f;
        }
        var foreignRegime = foreignCell.Controller.Get(d);
        if (foreignRegime is null) return 0f;
        var foreignAlliance = foreignRegime.GetAlliance(d);
        if (alliance.IsRivals(foreignAlliance, d) == false)
        {
            return 0f;
        }
        
        float mult = 1f;
        if (alliance.IsAtWar(foreignAlliance, d)) mult = 3f;
        var val = d.Context.PowerPoints[foreignCell] * mult;
        if (float.IsNaN(val))
        {
            throw new Exception();
        }
        return val;
    }
    
    
    
    public static Dictionary<FrontFace, float> GetFaceCosts(
        Alliance alliance,
        List<FrontFace> toCover,
        Data d)
    {
        if (toCover.Count == 0) return new Dictionary<FrontFace, float>();
        var totalEnemyCost = toCover.Sum(f => MilAiUtil.GetFaceEnemyCost(alliance, f, d));
        var totalLengthCost = toCover.Count;
        var enemyCostWeight = CoverOpposingWeight;
        var lengthCostWeight = CoverLengthWeight;
        return toCover
            .ToDictionary(f => f,
                f =>
                {
                    float enemyCost;
                    if (totalEnemyCost == 0f)
                    {
                        enemyCost = 0f;
                    }
                    else
                    {
                        enemyCost = enemyCostWeight * MilAiUtil.GetFaceEnemyCost(alliance, f, d) / totalEnemyCost;
                    }
                    var lengthCost = lengthCostWeight / totalLengthCost;
                    if (float.IsNaN(lengthCost))
                    {
                        throw new Exception($"length cost weight {lengthCostWeight} total length cost {totalLengthCost}");
                    }
                    var totalCost = enemyCost + lengthCost;
                    if (float.IsNaN(totalCost)) throw new Exception();
                    return totalCost;
                });
    }
    
    public static Dictionary<Army, HashSet<Cell>> 
        GetGroupLineAssignments(Alliance alliance,
            IEnumerable<Army> groups,
            List<FrontFace> faces,
            Data d)
    {
        var groupsInOrder = GetLineGroupsInOrder(faces,
            groups, d);
        var faceCosts = GetFaceCosts(alliance, faces, d);
        var lineOrders = Assigner.PickInOrderAndAssignAlongFaces(
            faces, 
            groupsInOrder, 
            u => u.GetPowerPoints(d),
            f => faceCosts[f]);
        return lineOrders.ToDictionary(kvp => kvp.Key,
            kvp => faces.GetRange(kvp.Value.X, kvp.Value.Y - kvp.Value.X + 1)
                .Select(f => f.GetNative(d)).ToHashSet());
    }
    public static List<Army> GetLineGroupsInOrder(List<FrontFace> faces,
        IEnumerable<Army> lineGroups,
        Data d)
    {
        var list = lineGroups.ToList();
        list.Sort((g, f) =>
        {
            var boundsG = g.GetCells(d);
            var gFirst = faces
                .FindIndex(f => boundsG.Contains(f.GetNative(d)));
            var gLast = faces
                .FindLastIndex(f => boundsG.Contains(f.GetNative(d)));

            var boundsF = f.GetCells(d);
            var fFirst = faces
                .FindIndex(f => boundsF.Contains(f.GetNative(d)));
            var fLast = faces
                .FindLastIndex(f => boundsF.Contains(f.GetNative(d)));

            if (gFirst == -1 || gLast == -1 || fFirst == -1 || fLast == -1)
            {
                return 0;
            }
            if (gFirst < fFirst) return -1;
            if (fFirst < gFirst) return 1;
            if (gLast < fLast) return -1;
            if (fLast < gLast) return 1;
            return 0;
        });
        return list;
    }
}