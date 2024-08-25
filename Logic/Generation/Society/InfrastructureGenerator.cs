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
    private GenKey _key;
    private float _portInfraNodeSize = 0f;
    private float _minSettlementSizeForInfraNode = 0f;
    private float _sizeBuildRoadRangeMult = 2.5f;
    private MultiTimer _multiTimer;
    public override GenReport Generate(GenKey key)
    {
        _key = key;
        _data = _key.GenData;
        _minSettlementSizeForInfraNode = key.Data.Models.Settlements.Town.MinSize;
        var genReport = new GenReport(nameof(InfrastructureGenerator));
        genReport.StartSection();
        _multiTimer = new MultiTimer();
        _multiTimer.AddName("poly level graph");
        _multiTimer.AddName("high level graph");
        _multiTimer.AddName("poly level traffic");
        _multiTimer.AddName("road segs");
        var roads = RoadNetwork.Create(key);
        
        // return genReport;

        var allSegs = new ConcurrentBag<Dictionary<Vector2I, RoadModel>>();
        
        Parallel.ForEach(_data.Planet.MapAux.LandSea.Landmasses, lm =>
        {
            var segs = BuildLmRoadNetwork(lm);
            if(segs != null) allSegs.Add(segs);
        });

        foreach (var segs in allSegs)
        {
            foreach (var kvp in segs)
            {
                var edge = kvp.Key;
                var road = kvp.Value;
                roads.Roads.Dic.Add(edge, road.MakeRef());
            }
        }
        
        genReport.StopSection(nameof(BuildLmRoadNetwork));
        _multiTimer.Print();
        return genReport;
    }
    private Dictionary<Vector2I, RoadModel> BuildLmRoadNetwork(Landmass lm)
    {
        var polyLvlGraph =
            _multiTimer.RunAndTime(
                () => GetPolyLevelGraph(lm.Polys), 
                "poly level graph");
        var hiLvlTrafficGraph
            = _multiTimer.RunAndTime(
                () => GetHighLevelTrafficGraph(polyLvlGraph),
                "high level graph");
            
        if (hiLvlTrafficGraph == null)
        {
            return new Dictionary<Vector2I, RoadModel>();
        }

        _multiTimer.RunAndTime(
            () => DoPolyLevelTraffic(polyLvlGraph, hiLvlTrafficGraph),
            "poly level traffic");
        
        return _multiTimer.RunAndTime(
            () => GetRoadSegs(polyLvlGraph), "road segs");
    }
    
    private Graph<InfrastructureNode, InfraNodeEdge> GetPolyLevelGraph(
        HashSet<MapPolygon> polys)
    {
        var urban = _data.Models.Landforms.Urban;
        var town = _data.Models.Settlements.Town;
        var city = _data.Models.Settlements.City;
        var graph = new Graph<InfrastructureNode, InfraNodeEdge>();
        var polyNodes = polys.ToDictionary(
            p => p,
            p =>
            {
                var cells = p.GetCells(_data);
                var urbanCells = cells
                    .Where(c => c.Landform.RefId == urban.Id);
                if (urbanCells.Any())
                {
                    var urbanCell = urbanCells.First();
                    var total = urbanCells.Sum(c => c.GetPeep(_data).Size);
                    var iNode = new InfrastructureNode(urbanCell, total);
                    return iNode;
                }
                else
                {
                    var centerCell = cells.MinBy(l => p.Center.Offset(l.GetCenter(), _data).LengthSquared());
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
            foreach (var nPoly in poly.Neighbors.Entities(_data))
            {
                if (nPoly.Id > poly.Id) continue;
                if (polyNodes.ContainsKey(nPoly) == false) continue;
                var polyCost = 1f;
                var cost = new InfraNodeEdge(polyCost, 0f);
                graph.AddEdge(polyNode, polyNodes[nPoly], cost);
            }
        }
        
        return graph;
    }

    private Graph<InfrastructureNode, InfraNodeEdge> GetHighLevelTrafficGraph(
        Graph<InfrastructureNode, InfraNodeEdge> polyLevelGraph)
    {

        var activeNodes = 
            polyLevelGraph.Elements
                .Where(e => e.Size > 0f)
                .ToList();
        if (activeNodes.Count < 3) return null;

        var relTo = activeNodes.First().Cell.GetCenter();
        var vGraph = VoronoiSandbox.DelaunayExt
            .GetVoronoiGraph(activeNodes,
                n => relTo.Offset(n.Cell.GetCenter(), _key.Data),
                (p, q) => (p, q));
        
        var hiLvlTrafficGraph = new Graph<InfrastructureNode, InfraNodeEdge>();

        foreach (var aNode in activeNodes)
        {
            hiLvlTrafficGraph.AddNode(aNode);
        }
        foreach (var (n1, n2) in vGraph.Edges)
        {
            if (hiLvlTrafficGraph.HasEdge(n1, n2)) continue;
            if (n1.Cell.GetCenter().Offset(n2.Cell.GetCenter(), _key.Data).LengthSquared() > 400f * 400f) continue;
            var traffic = n1.Size + n2.Size;
            var edge = new InfraNodeEdge(0f, traffic);
            hiLvlTrafficGraph.AddEdge(n1, n2, edge);
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
        var roadSegs = new Dictionary<Vector2I, RoadModel>();
        var dirt = _data.Models.RoadList.DirtRoad;
        var stone = _data.Models.RoadList.StoneRoad;
        var paved = _data.Models.RoadList.PavedRoad;
        var cellPaths = new Dictionary<Vector2I, List<Cell>>();
        polyLevelGraph.RemoveEdgesWhere(e => getRoadFromTraffic(e.Traffic) == null);
        
        var dic = polyLevelGraph.Elements
            .ToDictionary(n => n.Cell, n => n);
        
        polyLevelGraph.ForEachEdge((w, v, e) =>
        {
            if (v.Cell.Id > w.Cell.Id) return;
            var road = getRoadFromTraffic(e.Traffic);
            var path = getCellPath(w.Cell, v.Cell);
            if (path == null) return;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var key = from.GetIdEdgeKey(to);
                if (roadSegs.ContainsKey(key))
                {
                    var old = roadSegs[key];
                    if (road.CostOverride <= old.CostOverride)
                    {
                        roadSegs[key] = road;
                    }
                }
                else
                {
                    roadSegs.Add(key, road);
                }
            }
        });
        

        return roadSegs;
        List<Cell> getCellPath(Cell i1, Cell i2)
        {
            var key = i1.GetIdEdgeKey(i2);
            if (cellPaths.ContainsKey(key) == false)
            {
                addPaths(i1);
            }
            return cellPaths[key];
        }

        void addPaths(Cell w)
        {
            var node = dic[w];
            var ns = polyLevelGraph.GetNeighbors(node)
                .Where(n => cellPaths.ContainsKey(w.GetIdEdgeKey(n.Cell)) == false)
                .Select(n => n.Cell)
                .ToHashSet();
            
            var paths = 
                PathFinder<Cell>.FindMultiplePaths(
                w, ns, wp => wp.GetNeighbors(_data).Where(x => x is LandCell),
                (c1, c2) => PathFinder.RoadBuildEdgeCost(c1, c2, _data),
                (w, v) => w.GetCenter().Offset(v.GetCenter(), _data).LengthSquared() / (Cell.AvgCellDist * Cell.AvgCellDist));
            foreach (var kvp in paths)
            {
                var key = kvp.Key.GetIdEdgeKey(w);
                cellPaths.Add(key, kvp.Value);
            }
        }
        RoadModel getRoadFromTraffic(float traffic)
        {
            if (traffic > 200_000f) return paved;
            else if (traffic > 100_000f) return stone;
            else if (traffic > 1_000f) return dirt;
            return null;
        }
    }
}


