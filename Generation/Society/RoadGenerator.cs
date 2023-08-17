using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using Godot;

public class RoadGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = _key.GenData;
        var genReport = new GenReport(nameof(RoadGenerator));
        
        genReport.StartSection();
        
        var allSegs = new ConcurrentBag<IDictionary<Vector2, RoadModel>>();
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            allSegs.Add(GenerateForLandmass(lm));
        });
        foreach (var dic in allSegs)
        {
            foreach (var kvp in dic)
            {
                var edge = kvp.Key;
                var p1 = (MapPolygon)_data[(int) edge.X];
                var p2 = (MapPolygon)_data[(int) edge.Y];
                var polyEdge = p1.GetEdge(p2, _data);
                RoadSegment.Create(polyEdge, kvp.Value, _key);
            }
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }

    
    private IDictionary<Vector2, RoadModel> GenerateForLandmass(HashSet<MapPolygon> lm)
    {
        var settlementPolys = lm.Where(p => p.HasSettlement(_data));
        if (settlementPolys.Count() < 3) return new Dictionary<Vector2, RoadModel>();
        var first = lm.First();

        var polyPaths = new Dictionary<MapPolygonEdge, List<Waypoint>>();
        

        bool rail(Settlement s)
        {
            return s.Size >= 50f;
        }
        bool paved(Settlement s)
        {
            return s.Size >= 25f;
        }
        bool dirt(Settlement s)
        {
            return s.Size >= 5f;
        }
        var covered = new HashSet<Vector2>();
        var polySegs = new Dictionary<Vector2, RoadModel>();
        
        var railSettlements = settlementPolys.Where(p => rail(p.GetSettlement(_data))).ToList();
        if(railSettlements.Count > 2)
        {
            var railGraph = GraphGenerator.GenerateDelaunayGraph(railSettlements,
                s => first.GetOffsetTo(s, _data),
                (p1, p2) => p1.GetV2EdgeKey(p2));
            BuildRoadNetworkLocal(_data.Models.RoadList.Railroad, 2000f, 
                railGraph, covered, polySegs, true);
        }
        
        var pavedSettlements = settlementPolys.Where(p => paved(p.GetSettlement(_data))).ToList();
        if(pavedSettlements.Count > 2)
        {
            var pavedGraph = GraphGenerator.GenerateDelaunayGraph(pavedSettlements,
                s => first.GetOffsetTo(s, _data),
                (p1, p2) => p1.GetV2EdgeKey(p2));
            BuildRoadNetworkLocal(_data.Models.RoadList.PavedRoad, 1000f, 
                pavedGraph, covered, polySegs, true);
        }
        
        var dirtSettlements = settlementPolys.Where(p => dirt(p.GetSettlement(_data))).ToList();
        if(dirtSettlements.Count > 2)
        {
            var dirtGraph = GraphGenerator.GenerateDelaunayGraph(dirtSettlements,
                s => first.GetOffsetTo(s, _data),
                (p1, p2) => p1.GetV2EdgeKey(p2));
            BuildRoadNetworkLocal(_data.Models.RoadList.DirtRoad, 500f, 
                dirtGraph, covered, polySegs, true);
        }
        return polySegs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    private void BuildRoadNetworkLocal(RoadModel road, float roadBuildDist,
        IReadOnlyGraph<MapPolygon, Vector2> graph, HashSet<Vector2> covered,
        Dictionary<Vector2, RoadModel> segs, bool international)
    {
        var distSqr = roadBuildDist * roadBuildDist;
        foreach (var e in graph.Edges)
        {
            if (covered.Contains(e)) continue;
            covered.Add(e);
            var s1 = (MapPolygon)_data[(int)e.X];
            var s2 = (MapPolygon)_data[(int)e.Y];
            if (s1.GetOffsetTo(s2, _data).LengthSquared() > distSqr) continue;
            
            var buildPath = PathFinder.FindRoadBuildPath(s1, s2, road, _data, international);
            for (var i = 0; i < buildPath.Count - 1; i++)
            {
                var lo = buildPath[i].Id < buildPath[i + 1].Id
                    ? buildPath[i].Id
                    : buildPath[i + 1].Id;
                var pathEdge = buildPath[i].GetV2EdgeKey(buildPath[i + 1]);
                
                buildPath[i].GetEdge(buildPath[i + 1], _data);
                covered.Add(pathEdge);
                if(segs.ContainsKey(pathEdge)) continue;
                segs.Add(pathEdge, road);
            }
        }
    }

    private Dictionary<MapPolygonEdge, List<PolyTri>> GetTriPaths()
    {
        return null;
        var edges = _data.GetAll<MapPolygonEdge>()
            .Where(e => e.HighPoly.Entity(_data).IsLand && e.LowPoly.Entity(_data).IsLand);
    }
}
