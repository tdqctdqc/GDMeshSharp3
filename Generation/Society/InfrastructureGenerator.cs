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
    private float _portInfraNodeSize = 0f;
    private float _minSettlementSizeForInfraNode = 0f;
    private float _sizeBuildRoadRangeMult = .5f;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _data = _key.GenData;
        _minSettlementSizeForInfraNode = key.Data.Models.Settlements.Town.MinSize;
        var genReport = new GenReport(nameof(InfrastructureGenerator));
        
        genReport.StartSection();
        var roads = RoadNetwork.Create(key);
        var allSegs = new ConcurrentBag<Dictionary<Vector2I, RoadModel>>();

        Parallel.ForEach(_data.Planet.PolygonAux.LandSea.Landmasses, lm =>
        {
            var segs = GenerateForLandmass(lm);
            if(segs != null) allSegs.Add(segs);
        });
        foreach (var segs in allSegs)
        {
            foreach (var kvp in segs)
            {
                var edge = kvp.Key;
                var road = kvp.Value;
                var wp1 = MilitaryDomain.GetWaypoint((int)edge.X, _data);
                var wp2 = MilitaryDomain.GetWaypoint((int)edge.Y, _data);
                var success = roads.Roads.TryAdd(wp1, wp2, road.MakeRef());
                if (success == false) throw new Exception();
            }
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }
    private Dictionary<Vector2I, RoadModel> GenerateForLandmass(Landmass lm)
    {
        GeneratePorts(lm.Polys);
        return BuildLmRoadNetwork(lm);
    }


    
    private Dictionary<Vector2I, RoadModel> BuildLmRoadNetwork(Landmass lm)
    {
        
        var polyLvlGraph = GetPolyLevelGraph(lm.Polys);
        
        
        var hiLvlTrafficGraph = GetHighLevelTrafficGraph(polyLvlGraph);
        if (hiLvlTrafficGraph == null)
        {
            return new Dictionary<Vector2I, RoadModel>();
        }
        DoPolyLevelTraffic(polyLvlGraph, hiLvlTrafficGraph);
        
        var segs = GetRoadSegs(polyLvlGraph);
        
        return segs;
    }
    
    private Graph<InfrastructureNode, InfraNodeEdge> GetPolyLevelGraph(HashSet<MapPolygon> polys)
    {
        var urban = _data.Models.Landforms.Urban;
        var town = _data.Models.Settlements.Town;
        var city = _data.Models.Settlements.City;
        var graph = new Graph<InfrastructureNode, InfraNodeEdge>();
        
        var polyNodes = polys.ToDictionary(
            p => p,
            p =>
            {
                if (p.HasSettlement(_data)
                    && p.GetSettlement(_data).Tier.Model(_data).MinSize 
                        >= _minSettlementSizeForInfraNode
                    )
                {
                    var urbanTri = p.Tris.Tris.First(t => t.Landform(_data) == urban);
                    var found = _data.Military.WaypointGrid
                        .TryGetClosest(urbanTri.GetCentroid() + p.Center,
                            out var urbanWp, 
                            wp => wp is ILandWaypoint && wp.AssociatedWithPoly(p));
                    if (found == false) throw new Exception();
                    var iNode = new InfrastructureNode(urbanWp, p.GetPeep(_data).Size);
                    return iNode;
                }
                else
                {
                    var centerTri = p.Tris.Tris.FirstOrDefault(
                        t => t.Landform(_data) == urban);
                    Waypoint wp;
                    if (centerTri != null)
                    {
                        var found = _data.Military.WaypointGrid
                            .TryGetClosest(centerTri.GetCentroid() + p.Center,
                                out var urbanWp, 
                                wp => wp is ILandWaypoint && wp.AssociatedWithPoly(p));
                        if (found == false) throw new Exception();
                        wp = urbanWp;
                    }
                    else
                    {
                        wp = p.GetCenterWaypoint(_data);
                    }
                    var iNode = new InfrastructureNode(wp, 0f);
                    return iNode;
                }
            });
        var portNodes = GeneratePorts(polys)
            .Select(p => 
                new InfrastructureNode((Waypoint)p, 
                    _portInfraNodeSize))
            .ToHashSet();
        
        foreach (var kvp in polyNodes)
        {
            graph.AddNode(kvp.Value);
        }
        foreach (var portNode in portNodes)
        {
            graph.AddNode(portNode);
        }
        foreach (var portNode in portNodes)
        {
            var portPoly = portNode.Waypoint.AssocPolys(_data)
                .First(p => p.IsLand);
            var polyNode = polyNodes[portPoly];
            var polyCost = PathFinder.RoadBuildPolyCost(portPoly, _data);
            var cost = new InfraNodeEdge(polyCost, 0f,
                portNode.Waypoint.Pos.GetOffsetTo(polyNode.Waypoint.Pos, _data).Length());
            graph.AddEdge(portNode, polyNode, cost);
        }
        foreach (var kvp in polyNodes)
        {
            var polyNode = kvp.Value;
            var poly = kvp.Key;
            foreach (var nPoly in poly.Neighbors.Items(_data))
            {
                if (nPoly.Id > poly.Id) continue;
                if (polyNodes.ContainsKey(nPoly) == false) continue;
                var polyCost = PathFinder.RoadBuildPolyEdgeCost(poly, nPoly, _data);
                var cost = new InfraNodeEdge(polyCost, 0f,
                    poly.GetOffsetTo(nPoly, _data).Length());
                graph.AddEdge(polyNode, polyNodes[nPoly], cost);
            }
        }
        
        return graph;
    }

    private Graph<InfrastructureNode, InfraNodeEdge> GetHighLevelTrafficGraph(
        Graph<InfrastructureNode, InfraNodeEdge> polyPortGraph)
    {
        var activeNodes = 
            polyPortGraph.Elements
                .Where(e => e.Size > 0f)
                .ToList();
        if (activeNodes.Count < 3) return null;
        var relTo = activeNodes.First().Waypoint.Pos;
        var activeNodeGrid = new CylinderGrid<InfrastructureNode>(
            _data.Planet.Dim, 500f, i => i.Waypoint.Pos);
        var hiLvlTrafficGraph = new Graph<InfrastructureNode, InfraNodeEdge>();

        foreach (var aNode in activeNodes)
        {
            activeNodeGrid.Add(aNode);
            hiLvlTrafficGraph.AddNode(aNode);
        }
        foreach (var aNode in activeNodes)
        {
            var near = activeNodeGrid
                .GetWithin(aNode.Waypoint.Pos, 
                    aNode.Size * _sizeBuildRoadRangeMult, v => true);
            foreach (var nearNode in near)
            {
                if (nearNode == aNode) continue;
                if (hiLvlTrafficGraph.HasEdge(aNode, nearNode)) continue;
                var traffic = aNode.Size + nearNode.Size;
                
                
                var dist = aNode.Waypoint.Pos.GetOffsetTo(nearNode.Waypoint.Pos, _data).Length();

                var distMult = (10_000f - dist) / 10_000f;
                distMult = Mathf.Clamp(distMult, 0f, 1f);
                traffic *= distMult;
                var edge = new InfraNodeEdge(0f, traffic, dist);
                hiLvlTrafficGraph.AddEdge(aNode, nearNode, edge);
            }
        }
        
        
        return hiLvlTrafficGraph;
    }

    private void DoPolyLevelTraffic(
        Graph<InfrastructureNode, InfraNodeEdge> polyLevelGraph,
        Graph<InfrastructureNode, InfraNodeEdge> hiLevelTrafficGraph
        )
    {
        var hiLevelPaths = new Dictionary<Vector2I, List<InfrastructureNode>>();
        foreach (var hiLvlNode in hiLevelTrafficGraph.Elements)
        {
            var ns = hiLevelTrafficGraph.GetNeighbors(hiLvlNode);
            foreach (var nHiLvlNode in ns)
            {
                if (nHiLvlNode.Waypoint.Id > hiLvlNode.Waypoint.Id) continue;
                var hiLvlEdge = hiLevelTrafficGraph.GetEdge(hiLvlNode, nHiLvlNode);
                var path = getLoLvlPath(hiLvlNode, nHiLvlNode);
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var from = path[i];
                    var to = path[i + 1];
                    var oldVal = polyLevelGraph.GetEdge(from, to);
                    oldVal.Traffic += hiLvlEdge.Traffic;
                }
            }
        }
        
        List<InfrastructureNode> getLoLvlPath(InfrastructureNode i1, InfrastructureNode i2)
        {
            var key = i1.Waypoint.GetIdEdgeKey(i2.Waypoint);
            if (hiLevelPaths.ContainsKey(key)) return hiLevelPaths[key];
            var path = PathFinder
                .FindPathFromGraph(i1, i2,
                    polyLevelGraph, e => e.Cost,
                    iNode => iNode.Waypoint.Pos, _data
                );
            hiLevelPaths.Add(key, path);
            return path;
        }
    }

    private Dictionary<Vector2I, RoadModel> GetRoadSegs(
        Graph<InfrastructureNode, InfraNodeEdge> polyLevelGraph
        )
    {
        var edgeTraffic = new Dictionary<Vector2I, float>();
        var roadPolySegs = new Dictionary<Vector2I, RoadModel>();
        var roadWpSegs = new Dictionary<Vector2I, RoadModel>();
        var dirt = _data.Models.RoadList.DirtRoad;
        var stone = _data.Models.RoadList.StoneRoad;
        var paved = _data.Models.RoadList.PavedRoad;
        var wpPaths = new Dictionary<Vector2I, List<Waypoint>>();
        var walk = _data.Models.MoveTypes.InfantryMove;
        polyLevelGraph.RemoveEdges(e => getRoadFromTraffic(e.Traffic) == null);
        
        var dic = polyLevelGraph.Elements
            .ToDictionary(n => n.Waypoint, n => n);
        
        polyLevelGraph.ForEachEdge((w, v, e) =>
        {
            if (v.Waypoint.Id > w.Waypoint.Id) return;
            var road = getRoadFromTraffic(e.Traffic);
            var path = getWpPath(w.Waypoint, v.Waypoint);
            if (path == null) return;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var key = from.GetIdEdgeKey(to);
                if (roadWpSegs.ContainsKey(key))
                {
                    var old = roadWpSegs[key];
                    if (road.CostOverride <= old.CostOverride)
                    {
                        roadWpSegs[key] = road;
                    }
                }
                else
                {
                    roadWpSegs.Add(key, road);
                }
            }
        });
        

        return roadWpSegs;
        List<Waypoint> getWpPath(Waypoint i1, Waypoint i2)
        {
            var key = i1.GetIdEdgeKey(i2);
            if (wpPaths.ContainsKey(key) == false)
            {
                addPaths(i1);
            }
            return wpPaths[key];
        }

        void addPaths(Waypoint w)
        {
            var node = dic[w];
            var ns = polyLevelGraph.GetNeighbors(node)
                .Where(n => wpPaths.ContainsKey(w.GetIdEdgeKey(n.Waypoint)) == false)
                .Select(n => n.Waypoint)
                .ToHashSet();

            var paths = PathFinder<Waypoint>.FindMultiplePaths(
                w, ns, wp => wp.GetNeighbors(_data).Where(x => x is ILandWaypoint),
                getEdgeCost, (w, v) => w.Pos.GetOffsetTo(v.Pos, _data).Length());
            foreach (var kvp in paths)
            {
                var key = kvp.Key.GetIdEdgeKey(w);
                wpPaths.Add(key, kvp.Value);
            }
        }
        RoadModel getRoadFromTraffic(float traffic)
        {
            if (traffic > 100_000f) return paved;
            else if (traffic > 50_000f) return stone;
            else if (traffic > 1_000f) return dirt;
            return null;
        }

        float getEdgeCost(Waypoint w, Waypoint v)
        {
            var key = w.GetIdEdgeKey(v);
            if (edgeTraffic.TryGetValue(key, out var traffic))
            {
                var road = getRoadFromTraffic(traffic);
                if (road != null)
                {
                    var length = w.Pos.GetOffsetTo(v.Pos, _data).Length();
                    return length / road.CostOverride;
                }
            }
            return PathFinder.RoadBuildEdgeCost(w, v, _data);
        }
    }
    private HashSet<IWaypoint> GeneratePorts(HashSet<MapPolygon> polys)
    {
        var uf = new UnionFind<MapPolygon>(
            (p, n) => p.OwnerRegime.RefId == n.OwnerRegime.RefId);
         
        foreach (var poly in polys)
        {
            uf.AddElement(poly, poly.Neighbors.Items(_key.Data));
        }
        var res = new HashSet<IWaypoint>();

        var fragments = uf.GetUnions();
        foreach (var fragment in fragments)
        {
            res.AddRange(GenerateRegimeLmFragmentPorts(fragment));
        }

        return res;
    }
    private HashSet<IWaypoint> GenerateRegimeLmFragmentPorts(List<MapPolygon> polys)
    {
        var coastCityPolys = polys
            .Where(p => p.HasSettlement(_key.Data))
            .Where(p => p.Neighbors.Items(_key.Data)
                .Any(n => n.IsWater()));
        var res = new HashSet<IWaypoint>();
        foreach (var poly in coastCityPolys)
        {
            var wps = poly.GetAssocTacWaypoints(_key.Data)
                .OfType<ICoastWaypoint>();
            var polySeaIds = new HashSet<int>();
            foreach (var coastWaypoint in wps)
            {
                if (polySeaIds.Contains(coastWaypoint.Sea)) continue;
                polySeaIds.Add(coastWaypoint.Sea);
                coastWaypoint.SetPort(true, _key);
                res.Add(coastWaypoint);
            }
        }

        return res;
    }
}


