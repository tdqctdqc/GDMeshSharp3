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
        
        var segs = new ConcurrentDictionary<Vector2, RoadModel>();
        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            GenerateForLandmass(lm, segs);
        });
        
        foreach (var kvp in segs)
        {
            var edge = kvp.Key;
            var road = kvp.Value;
            
            var wp1 = MilitaryDomain.GetTacWaypoint((int)edge.X, _data);
            var wp2 = MilitaryDomain.GetTacWaypoint((int)edge.Y, _data);
            roads.Roads.TryAdd(wp1, wp2, road.MakeRef());
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }

    private void AddSeg(RoadModel r, Vector2 edgeKey, IDictionary<Vector2, RoadModel> segs)
    {
        if (segs.ContainsKey(edgeKey) == false
            || r.SpeedMult > segs[edgeKey].SpeedMult)
        {
            segs[edgeKey] = r;
        }
    }
    private void GenerateForLandmass(Landmass lm,
        IDictionary<Vector2, RoadModel> segs)
    {
        GeneratePorts(lm);
        BuildLmRoadNetwork(lm, segs);
        BuildPortRoads(lm, segs);
    }
    private void BuildLmRoadNetwork(Landmass lm,
        IDictionary<Vector2, RoadModel> segs)
    {
        var regimeFragments = UnionFind.Find(lm.Polys,
            (p, q) => p.OwnerRegime.RefId == q.OwnerRegime.RefId,
            p => p.Neighbors.Items(_key.Data));
        var city = _data.Models.Settlements.City;
        var town = _data.Models.Settlements.Town;
        var dirt = _key.Data.Models.RoadList.DirtRoad;
        var stone = _key.Data.Models.RoadList.StoneRoad;
        var settlements = lm.Polys
            .Where(p => p.HasSettlement(_key.Data))
            .Select(p => p.GetSettlement(_key.Data))
            .ToList();

        var towns = settlements
            .Where(s => s.Poly.Entity(_data).GetPeep(_data).Size >= town.MinSize)
            .ToList();
        
        if(towns.Count() > 2)
        {
            var dirtRoads = BuildRoadLevel(towns);
            foreach (var edgeKey in dirtRoads)
            {
                AddSeg(dirt, edgeKey, segs);
            }
        }

        var big = settlements
            .Where(s => s.Poly.Entity(_data).GetPeep(_data).Size >= city.MinSize)
            .ToList();
        if(big.Count() > 2)
        {
            var stoneRoads = BuildRoadLevel(big);
            foreach (var edgeKey in stoneRoads)
            {
                AddSeg(stone, edgeKey, segs);
            }
        }
    }

    private List<Vector2> BuildRoadLevel(List<Settlement> settlements)
    {
        var first = settlements.First();
        var graph = GraphGenerator.GenerateDelaunayGraph(
            settlements,
            s => first.Poly.Entity(_data).GetOffsetTo(s.Poly.Entity(_data), _data),
            (p1, p2) => p1.GetIdEdgeKey(p2));
        var res = new List<Vector2>();
        var tWps = _data.Military.TacticalWaypoints;

        foreach (var e in graph.Edges)
        {
            var s1 = (Settlement)_data[(int)e.X];
            var s2 = (Settlement)_data[(int)e.Y];
            var buildPath = PathFinder.FindRoadBuildPolyPath(s1.Poly.Entity(_data), 
                s2.Poly.Entity(_data), 
                _data, true);

            if (buildPath.Any(p => p.IsWater())) throw new Exception();
            for (var i = 0; i < buildPath.Count - 1; i++)
            {
                var waypoints = tWps.GetPolyPath(buildPath[i], buildPath[i + 1]).ToList();
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
            (p, n) => p.OwnerRegime.RefId == n.OwnerRegime.RefId);
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
        var coastCityPolys = polys
            .Where(p => p.HasSettlement(_key.Data))
            .Where(p => p.Neighbors.Items(_key.Data)
                .Any(n => n.IsWater()));
        
        foreach (var poly in coastCityPolys)
        {
            var wps = poly.GetAssocTacWaypoints(_key.Data)
                .WhereOfType<ICoastWaypoint>();
            var polySeaIds = new HashSet<int>();
            foreach (var coastWaypoint in wps)
            {
                if (polySeaIds.Contains(coastWaypoint.Sea)) continue;
                polySeaIds.Add(coastWaypoint.Sea);
                coastWaypoint.SetPort(true, _key);
            }
        }
    }
    private void BuildPortRoads(Landmass lm, 
        IDictionary<Vector2, RoadModel> segs)
    {
        var stoneRoad = _key.Data.Models.RoadList.StoneRoad;
        var dirtRoad = _key.Data.Models.RoadList.DirtRoad;
        var city = _data.Models.Settlements.City;
        var town = _data.Models.Settlements.Town;
        var village = _data.Models.Settlements.Village;
        
        var settlementPolys = lm.Polys
            .Where(p => p.HasSettlement(_data));
        var largeSettlementPolys =
            settlementPolys.Where(p => p.GetSettlement(_data).Tier.Model(_data) != village);
        if (settlementPolys.Count() < 3) return;

        var portWps = lm.Polys
            .SelectMany(p => p.GetAssocTacWaypoints(_data))
            .Distinct().Where(wp => wp is CoastWaypoint c && c.Port);

        var settlementWps = settlementPolys
            .Select(p => p.GetCenterWaypoint(_data)).ToHashSet();
        var largeSettlementWps = largeSettlementPolys
            .Select(p => p.GetCenterWaypoint(_data)).ToHashSet();
        foreach (var portWp in portWps)
        {
            var path = PathFinder<Waypoint>.FindPathMultipleEnds(portWp,
                wp => settlementWps.Contains(wp),
                wp => wp.TacNeighbors(_data)
                    .Where(n => n is SeaWaypoint == false
                    && (n is IRiverWaypoint r ? r.Bridgeable : true)),
                (w, v) => PathFinder.LandEdgeCost(w, v, _data));
            if (path == null) continue;
            var settlement = path.Last().AssocPolys(_key.Data).First().GetSettlement(_key.Data);
            

            if (settlement.Tier.Model(_data) == village)
            {
                var pathToLargeSettlement = PathFinder<Waypoint>.FindPathMultipleEnds(portWp,
                    wp => largeSettlementWps.Contains(wp),
                    wp => wp.TacNeighbors(_data)
                        .Where(n => n is SeaWaypoint == false
                        && (n is IRiverWaypoint r ? r.Bridgeable : true)),
                    (w, v) => PathFinder.LandEdgeCost(w, v, _data));
                if (pathToLargeSettlement != null)
                {
                    for (var i = 0; i < pathToLargeSettlement.Count; i++)
                    {
                        path.Add(pathToLargeSettlement[i]);
                    }
                }
            }
            
            for (var i = 0; i < path.Count - 1; i++)
            {
                RoadModel use = settlement.Poly.Entity(_data).GetPeep(_data).Size >= city.MinSize
                    ? stoneRoad
                    : dirtRoad;
                AddSeg(use, path[i].GetIdEdgeKey(path[i + 1]), segs);
            }
        }
    }
}
