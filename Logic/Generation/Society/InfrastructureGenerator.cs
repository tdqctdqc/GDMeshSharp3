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
                var wp1 = PlanetDomainExt.GetPolyCell(edge.X, _data);
                var wp2 = PlanetDomainExt.GetPolyCell(edge.Y, _data);
                var success = roads.Roads.TryAdd(wp1, wp2, road.MakeRef());
                if (success == false) throw new Exception();
            }
        }
        genReport.StopSection(nameof(GenerateForLandmass));
        return genReport;
    }
    private Dictionary<Vector2I, RoadModel> GenerateForLandmass(Landmass lm)
    {
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
                var landCells = p.GetCells(_data);
                if (p.HasSettlement(_data)
                    && p.GetSettlement(_data).Tier.Model(_data).MinSize 
                        >= _minSettlementSizeForInfraNode
                    )
                {
                    var urbanCell = landCells
                        .First(t => t.GetLandform(_data) == urban);
                    var iNode = new InfrastructureNode(urbanCell, p.GetPeep(_data).Size);
                    return iNode;
                }
                else
                {
                    var centerCell = landCells.FirstOrDefault(
                        t => t.GetLandform(_data) == urban);
                    if (centerCell == null)
                    {
                        centerCell = landCells.MinBy(l => p.Center.GetOffsetTo(l.GetCenter(), _data).Length());
                    }
                    var iNode = new InfrastructureNode(centerCell, 0f);
                    return iNode;
                }
            });
        
        foreach (var kvp in polyNodes)
        {
            graph.AddNode(kvp.Value);
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
        var activeNodeGrid = new CylinderGrid<InfrastructureNode>(
            _data.Planet.Dim, 500f, i => i.Cell.GetCenter());
        var hiLvlTrafficGraph = new Graph<InfrastructureNode, InfraNodeEdge>();

        foreach (var aNode in activeNodes)
        {
            activeNodeGrid.Add(aNode);
            hiLvlTrafficGraph.AddNode(aNode);
        }
        foreach (var aNode in activeNodes)
        {
            var near = activeNodeGrid
                .GetWithin(aNode.Cell.GetCenter(), 
                    aNode.Size * _sizeBuildRoadRangeMult, v => true);
            foreach (var nearNode in near)
            {
                if (nearNode == aNode) continue;
                if (hiLvlTrafficGraph.HasEdge(aNode, nearNode)) continue;
                var traffic = aNode.Size + nearNode.Size;
                
                
                var dist = aNode.Cell.GetCenter().GetOffsetTo(nearNode.Cell.GetCenter(), _data).Length();

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
                if (nHiLvlNode.Cell.Id > hiLvlNode.Cell.Id) continue;
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
            var key = i1.Cell.GetIdEdgeKey(i2.Cell);
            if (hiLevelPaths.ContainsKey(key)) return hiLevelPaths[key];
            var path = PathFinder
                .FindPathFromGraph(i1, i2,
                    polyLevelGraph, e => e.Cost,
                    iNode => iNode.Cell.GetCenter(), _data
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
        var wpPaths = new Dictionary<Vector2I, List<PolyCell>>();
        var walk = _data.Models.MoveTypes.InfantryMove;
        polyLevelGraph.RemoveEdgesWhere(e => getRoadFromTraffic(e.Traffic) == null);
        
        var dic = polyLevelGraph.Elements
            .ToDictionary(n => n.Cell, n => n);
        
        polyLevelGraph.ForEachEdge((w, v, e) =>
        {
            if (v.Cell.Id > w.Cell.Id) return;
            var road = getRoadFromTraffic(e.Traffic);
            var path = getWpPath(w.Cell, v.Cell);
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
        List<PolyCell> getWpPath(PolyCell i1, PolyCell i2)
        {
            var key = i1.GetIdEdgeKey(i2);
            if (wpPaths.ContainsKey(key) == false)
            {
                addPaths(i1);
            }
            return wpPaths[key];
        }

        void addPaths(PolyCell w)
        {
            var node = dic[w];
            var ns = polyLevelGraph.GetNeighbors(node)
                .Where(n => wpPaths.ContainsKey(w.GetIdEdgeKey(n.Cell)) == false)
                .Select(n => n.Cell)
                .ToHashSet();

            var paths = 
                PathFinder<PolyCell>.FindMultiplePaths(
                w, ns, wp => wp.GetNeighbors(_data).Where(x => x is LandCell),
                getEdgeCost, (w, v) => w.GetCenter().GetOffsetTo(v.GetCenter(), _data).Length());
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

        float getEdgeCost(PolyCell w, PolyCell v)
        {
            var key = w.GetIdEdgeKey(v);
            if (edgeTraffic.TryGetValue(key, out var traffic))
            {
                var road = getRoadFromTraffic(traffic);
                if (road != null)
                {
                    var length = w.GetCenter().GetOffsetTo(v.GetCenter(), _data).Length();
                    return length / road.CostOverride;
                }
            }
            return PathFinder.RoadBuildEdgeCost(w, v, _data);
        }
    }
}


