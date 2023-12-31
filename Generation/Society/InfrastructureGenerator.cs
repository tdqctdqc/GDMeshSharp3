using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using Godot;
using RoadWaypointGraph = Graph<(Waypoint node, float size), 
    (Godot.Vector2I key, float cost, float traffic)>;

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
                var wp1 = MilitaryDomain.GetTacWaypoint((int)edge.X, _data);
                var wp2 = MilitaryDomain.GetTacWaypoint((int)edge.Y, _data);
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
        return BuildLmRoadNetworkNew(lm);
    }
    
    private Dictionary<Vector2I, RoadModel> BuildLmRoadNetworkNew(Landmass lm)
    {
        var roadNodes = GetRoadNodes(lm.Polys);
        if (roadNodes.Count < 5) return null;
        try
        {
            var graph = MakeRoadWaypointGraph(roadNodes);
            var res = MakeRoadSegs(graph);
            GD.Print("success");
            return res;
        }
        catch (Exception e)
        {
            GD.Print("failure");
            return null;
        }
    }
    
    
    
    
    private List<(Waypoint, float size)> GetRoadNodes(HashSet<MapPolygon> polys)
    {
        var urban = _data.Models.Landforms.Urban;
        var town = _data.Models.Settlements.Town;

        var settlements = polys
            .Where(p => p.HasSettlement(_key.Data))
            .Select(p =>
            {
                var urbanTri = p.Tris.Tris.First(t => t.Landform(_data) == urban);
                var found = _data.Military.WaypointGrid
                    .TryGetClosest(urbanTri.GetCentroid() + p.Center,
                        out var urbanWp, 
                        wp => wp is ILandWaypoint && wp.AssociatedWithPoly(p));
                if (found == false) throw new Exception();
                return (urbanWp, (float)p.GetPeep(_data).Size);
            })
            .ToList();
        var ports = GeneratePorts(polys)
            .Select(p => ((Waypoint)p, (float)town.MinSize));
        settlements.AddRange(ports);
        return settlements;
    }
    
    private RoadWaypointGraph MakeRoadWaypointGraph(List<(Waypoint wp, float size)> roadNodes)
        {
            var relTo = roadNodes.First().wp.Pos;
            var graph = GraphGenerator
                .GenerateDelaunayGraph<(Waypoint node, float size), 
                    (Vector2I key, float cost, float traffic)>(
                roadNodes,
                s => relTo.GetOffsetTo(s.node.Pos, _data),
                getEdge
            );

            (Vector2I key, float cost, float traffic)
                getEdge((Waypoint node, float size) node1, (Waypoint node, float size) node2)
            {
                var key = node1.node.GetIdEdgeKey(node2.node);
                var length = node1.node.Pos.GetOffsetTo(node2.node.Pos, _data).Length(); 
                if (length > 1000f)
                {
                    return (-Vector2I.One, 0f, 0f);
                }
                return (key, length, 1001f);
            }
            
            foreach (var el in graph.Elements)
            {
                var ns = graph
                    .GetNeighbors(el).ToArray();
                foreach (var n in ns)
                {
                    var edge = graph.GetEdge(el, n);
                    if (edge.key == -Vector2I.One)
                    {
                        graph.RemoveEdge(el, n);
                    }
                }
            }
    
            return graph;
        }
        
    
    private Dictionary<Vector2I, RoadModel> MakeRoadSegs(
        RoadWaypointGraph graph)
    {
        var frontier = new HashSet<Waypoint>();
        var edgeTraffic = new Dictionary<Vector2I, float>();
        foreach (var graphNode in graph.Nodes)
        {
            var size = graphNode.Element.size;
            var credit = size * 100f;
        }

        var dirt = _data.Models.RoadList.DirtRoad;
        var stone = _data.Models.RoadList.StoneRoad;

        foreach (var valueTuple in graph.Edges)
        {
            var traffic = valueTuple.traffic;
            var key = valueTuple.key;
            var from = MilitaryDomain.GetTacWaypoint(key.X, _data);
            var to = MilitaryDomain.GetTacWaypoint(key.Y, _data);
            var path = PathFinder<Waypoint>
                .FindPath(from, to,
                    wp => wp.TacNeighbors(_data),
                    getEdgeCost,
                    (w, v) => w.Pos.GetOffsetTo(v.Pos, _data).Length()
                );
            if (path == null) continue;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var pathEdgeKey = path[i].GetIdEdgeKey(path[i + 1]);
                edgeTraffic.AddOrSum(pathEdgeKey, traffic);
            }
        }
        var segs = new Dictionary<Vector2I, RoadModel>();
        foreach (var kvp in edgeTraffic)
        {
            var road = getRoadFromTraffic(kvp.Value);
            if(road != null) segs.Add(kvp.Key, road);
        }

        RoadModel getRoadFromTraffic(float traffic)
        {
            if (traffic > 1000f) return stone;
            else if (traffic > 500f) return dirt;
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
                    return length / road.SpeedMult;
                }
            }

            return PathFinder.RoadBuildEdgeCost(w, v, _data);
        }
        return segs;
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
                .WhereOfType<ICoastWaypoint>();
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
