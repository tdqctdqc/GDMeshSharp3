using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using Godot;

public class InfrastructureGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = _key.GenData;
        var genReport = new GenReport(nameof(InfrastructureGenerator));
        
        genReport.StartSection();
        var roads = RoadNetwork.Create(key);
        var nav = key.Data.Planet.Nav;
        
        var allSegs = new ConcurrentBag<IDictionary<RoadModel, HashSet<Vector2>>>();
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            allSegs.Add(GenerateForLandmass(lm));
        });
        foreach (var dic in allSegs)
        {
            foreach (var kvp in dic)
            {
                var road = kvp.Key.MakeRef();
                var edges = kvp.Value;
                foreach (var edge in edges)
                {
                    var wp1 = _data.Planet.Nav.Waypoints[(int) edge.X];
                    var wp2 = _data.Planet.Nav.Waypoints[(int) edge.Y];
                    roads.Roads.TryAdd(wp1, wp2, road);
                }
            }
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }
    private IDictionary<RoadModel, HashSet<Vector2>> GenerateForLandmass(Landmass lm)
    {
        GeneratePorts(lm);
        var res = BuildLmRoadNetwork(lm);
        BuildPortRoads(lm, res);
        return res;
    }
    private Dictionary<RoadModel, HashSet<Vector2>> BuildLmRoadNetwork(Landmass lm)
    {
        var regimeFragments = UnionFind.Find(lm.Polys,
            (p, q) => p.Regime.RefId == q.Regime.RefId,
            p => p.Neighbors.Items(_key.Data));
        
        var res = new Dictionary<RoadModel, HashSet<Vector2>>();
        var dirt = _key.Data.Models.RoadList.DirtRoad;
        res.Add(dirt, new HashSet<Vector2>());
        var stone = _key.Data.Models.RoadList.StoneRoad;
        res.Add(stone, new HashSet<Vector2>());
        var settlements = lm.Polys
            .Where(p => p.HasSettlement(_key.Data))
            .Select(p => p.GetSettlement(_key.Data))
            .ToList();
        if(settlements.Count() > 2)
        {
            var dirtRoads = BuildRoadLevel(settlements);
            res[dirt].AddRange(dirtRoads);
        }

        var big = settlements.Where(s => s.Size > 35).ToList();
        if(big.Count() > 2)
        {
            var stoneRoads = BuildRoadLevel(big);
            res[stone].AddRange(stoneRoads);
            stoneRoads.ForEach(v => res[dirt].Remove(v));
        }

        return res;
    }

    private List<Vector2> BuildRoadLevel(List<Settlement> settlements)
    {
        var first = settlements.First();
        var nav = _data.Planet.Nav;
        var graph = GraphGenerator.GenerateDelaunayGraph(
            settlements,
            s => first.Poly.Entity(_data).GetOffsetTo(s.Poly.Entity(_data), _data),
            (p1, p2) => p1.GetIdEdgeKey(p2));
        var res = new List<Vector2>();
        foreach (var e in graph.Edges)
        {
            var s1 = (Settlement)_data[(int)e.X];
            var s2 = (Settlement)_data[(int)e.Y];
            
            var buildPath = PathFinder.FindRoadBuildPath(s1.Poly.Entity(_data), 
                s2.Poly.Entity(_data), 
                _data, true);

            if (buildPath.Any(p => p.IsWater())) throw new Exception();
            for (var i = 0; i < buildPath.Count - 1; i++)
            {
                var waypoints = nav.GetPolyPath(buildPath[i], buildPath[i + 1]).ToList();
                for (var k = 0; k < waypoints.Count - 1; k++)
                {
                    var edgeId = waypoints[k].GetIdEdgeKey(waypoints[k + 1]);
                    res.Add(edgeId);
                }
            }
        }

        return res;
    }
    
    
    private void GeneratePorts(Landmass lm)
    {
        var uf = new UnionFind<MapPolygon>(
            (p, n) => p.Regime.RefId == n.Regime.RefId);
        foreach (var poly in lm.Polys)
        {
            uf.AddElement(poly, poly.Neighbors.Items(_key.Data));
        }

        var fragments = uf.GetUnions();
        foreach (var polys in fragments)
        {
            GenerateRegimeLmFragmentPorts(polys);
        }
    }
    private void GenerateRegimeLmFragmentPorts(List<MapPolygon> polys)
    {
        var coasts = new Dictionary<Sea, HashSet<MapPolygon>>();
        var seas = _key.Data.Planet.PolygonAux.LandSea.SeaDic;

        var coastPolys = polys
            .Where(p => p.Neighbors.Items(_key.Data)
                .Any(n => n.IsWater()));
        var coastNavs = coastPolys
                .SelectMany(p => p.GetAssocWaypoints(_key.Data))
            .Where(wp => wp.WaypointData.Value() is CoastNav)
            .Where(wp => wp.NumAssocPolys() == 2)
            .Distinct()
            .SortInto(wp => ((CoastNav)wp.WaypointData.Value()).Sea);

        var navPointsPerPort = 10;
        foreach (var kvp in coastNavs)
        {
            var navPs = kvp.Value;
            if (navPs.Count() == 0) continue;
            var numPorts = Mathf.CeilToInt(navPs.Count() / navPointsPerPort);
            numPorts = Mathf.Max(1, numPorts);
            var portNavs = navPs.GetDistinctRandomElements(numPorts);
            foreach (var portNav in portNavs)
            {
                ((CoastNav)portNav.WaypointData.Value()).SetPort(true, _key);
            }
        }
    }

    private void BuildPortRoads(Landmass lm, Dictionary<RoadModel,HashSet<Vector2>> res)
    {
        var stoneRoad = _key.Data.Models.RoadList.StoneRoad;
        var dirtRoad = _key.Data.Models.RoadList.DirtRoad;
        var nav = _key.Data.Planet.Nav;
        
        if (res.ContainsKey(stoneRoad) == false)
        {
            res.Add(stoneRoad, new HashSet<Vector2>());
        }
        var settlementPolys = lm.Polys
            .Where(p => p.HasSettlement(_data));
        if (settlementPolys.Count() < 3) return;

        var portWps = lm.Polys
            .SelectMany(p => _key.Data.Planet.Nav.GetPolyAssocWaypoints(p, _key.Data))
            .Distinct().Where(wp => wp.WaypointData.Value() is CoastNav c && c.Port);

        var settlementWps = settlementPolys
            .Select(p => nav.GetPolyCenterWaypoint(p)).ToHashSet();
        
        foreach (var portWp in portWps)
        {
            var path = PathFinder<Waypoint>.FindPathMultipleEnds(portWp,
                wp => settlementWps.Contains(wp),
                wp => wp.Neighbors.Select(n => nav.Waypoints[n])
                    .Where(n => n.WaypointData.Value() is SeaNav == false),
                (w, v) => PathFinder.LandEdgeCost(w, v, _data));
            if (path == null) continue;
            for (var i = 0; i < path.Count - 1; i++)
            {
                res[stoneRoad].Add(path[i].GetIdEdgeKey(path[i + 1]));
                res[dirtRoad].Remove(path[i].GetIdEdgeKey(path[i + 1]));
            }
        }
    }
}
