using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Priority_Queue;

public class LocationGenerator : Generator
{
    public GenData Data { get; private set; }
    private GenWriteKey _key;
    private IdDispenser _id;
    public LocationGenerator()
    {
    }

    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
        _id = key.IdDispenser;
        Data = key.GenData;
        report.StartSection();
        GenerateCities();
        NameSettlements(key.Data);
        report.StopSection("Generating Cities");

        return report;
    }
    
    private void GenerateCities()
    {
        var landPolys = Data.Planet.Polygons.Entities.Where(p => p.IsLand);
        var unions = UnionFind.Find(landPolys.ToList(), 
            (p, q) => p.Regime.Entity(Data) == q.Regime.Entity(Data),
            p => p.Neighbors.Entities(Data));

        var dic = new ConcurrentDictionary<List<MapPolygon>, List<int>>();
        
        Parallel.ForEach(unions, u =>
        {
            var res = PregenerateSettlements(u);
            dic.TryAdd(res.settlementPolys, res.settlementSizes);
        });
        foreach (var kvp in dic)
        {
            CreateSettlements(kvp.Key, kvp.Value);
        }
        Parallel.ForEach(dic, kvp =>
        {
            SetUrbanTris(kvp.Key, kvp.Value);
        });
        Parallel.ForEach(unions, u =>
        {
            Deforest(u);
        });
    }
    
    private (List<MapPolygon> settlementPolys, List<int> settlementSizes)
        PregenerateSettlements(List<MapPolygon> regimeUnionPolys)
    {
        // float minSettlementScore = 1f;
        float scorePerSettlement = 1f;
        var numSettlements = regimeUnionPolys.Count / 10;
        if (numSettlements == 0) numSettlements = 1;
        var score = regimeUnionPolys.Sum(PopScore);
        
        var polyQueue = new SimplePriorityQueue<MapPolygon>();
        for (var i = 0; i < regimeUnionPolys.Count; i++)
        {
            var p = regimeUnionPolys[i];
            
            
            if (p.Tris.Tris.Any(t => t.Landform != LandformManager.Mountain
                           && t.Landform != LandformManager.Peak
                           && t.Landform != LandformManager.River))
            {
                polyQueue.Enqueue(p, -SettlementDesireability(p));
            }
        }
        numSettlements = Math.Max(numSettlements, Mathf.FloorToInt(score / scorePerSettlement));
        numSettlements = Math.Min(numSettlements, polyQueue.Count);
        var settlementPolys = new List<MapPolygon>();
        var forbidden = new HashSet<MapPolygon>();

        for (var i = 0; i < numSettlements; i++)
        {
            if (polyQueue.Count == 0) break;
            var poly = polyQueue.Dequeue();
            if (forbidden.Contains(poly)) continue;
            foreach (var n in poly.Neighbors.Entities(Data))
            {
                forbidden.Add(n);
            }
            settlementPolys.Add(poly);
        }
        numSettlements = Math.Min(numSettlements, settlementPolys.Count);
        var settlementSizes = new List<int>();

        var tierSize = score;
        var tier = 1;
        while (settlementSizes.Count < numSettlements)
        {
            tierSize = score / (tier * tier);

            for (var i = 0; i < tier; i++)
            {
                if (settlementSizes.Count >= numSettlements) break;
                settlementSizes.Add(Mathf.RoundToInt(tierSize));
            }
            tier++;
        }
        

        return (settlementPolys, settlementSizes);
    }

    private void CreateSettlements(List<MapPolygon> settlementPolys, List<int> settlementSizes)
    {
        var numSettlements = Mathf.Min(settlementPolys.Count, settlementSizes.Count);
        for (var i = 0; i < numSettlements; i++)
        {
            var p = settlementPolys[i];
            var size = settlementSizes[i];
            Settlement.Create( 
                "Doot", 
                p, size, _key);
        }
    }
    private void SetUrbanTris(List<MapPolygon> settlementPolys, List<int> settlementSizes)
    {
        var numSettlements = Mathf.Min(settlementPolys.Count, settlementSizes.Count);
        var sizeForFirstTri = 10f;
        
        for (var i = 0; i < numSettlements; i++)
        {
            var p = settlementPolys[i];
            var size = settlementSizes[i];

            var availTris = p.Tris.Tris
                .Where(t => t.Landform != LandformManager.River
                    && t.Landform != LandformManager.Mountain
                    && t.Landform != LandformManager.Peak)
                .OrderBy(t => t.GetCentroid().LengthSquared());
            
            var numUrbanTris = Mathf.Min(availTris.Count(), trisForSize(size));
            for (var j = 0; j < numUrbanTris; j++)
            {
                availTris.ElementAt(j).SetLandform(LandformManager.Urban, _key);
                availTris.ElementAt(j).SetVegetation(VegetationManager.Barren, _key);
            }
        }
        int trisForSize(float size)
        {
            return Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(size / sizeForFirstTri)));
        }
    }
    private void Deforest(List<MapPolygon> polys)
    {
        foreach (var poly in polys)
        {
            var forestTris = poly.Tris.Tris.Where(t => t.Vegetation == VegetationManager.Forest);
            if (forestTris.Count() == 0)
            {
                continue;
            }
            float deforestStr = 0f;
            if (poly.HasSettlement(Data))
            {
                deforestStr = .5f;
            }
            else if (poly.Neighbors.Entities(Data).Any(n => n.HasSettlement(Data)))
            {
                deforestStr = .1f;
            }
            else continue;
            foreach (var tri in forestTris)
            {
                if (Game.I.Random.Randf() < deforestStr)
                {
                    tri.SetVegetation(VegetationManager.Grassland, _key);
                }
            }
        }
        
    }
    
    private float PopScore(MapPolygon poly)
    {
        return 2f * (poly.Moisture + (1f - poly.Roughness * .5f));
    }

    private float SettlementDesireability(MapPolygon poly)
    {
        var res = PopScore(poly);
        if (poly.Tris.Tris.Any(t => t.Landform == LandformManager.River))
        {
            res += 1f;
        }
        if (poly.IsCoast(Data))
        {
            res += 1f;
        }
        return res;
    }
    
    private void NameSettlements(Data data)
    {
        var taken = new HashSet<string>();
        foreach (var r in data.Society.Regimes.Entities)
        {
            var settlements = r.Polygons.Entities(Data).Where(p => p.HasSettlement(data))
                .Select(p => p.GetSettlement(data));
            var names = r.Culture.Model(data).SettlementNames.Where(n => taken.Contains(n) == false).ToList();
            if (settlements.Count() > names.Count) continue;
            int iter = 0;
            foreach (var settlement in settlements)
            {
                taken.Add(names[iter]);
                settlement.SetName(names[iter], _key);
                iter++;
            }
        }
    }
}