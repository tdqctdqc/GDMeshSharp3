using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class TacticalWaypointGenerator : Generator
{
    private IdDispenser _id;
    private float _waypointDist = 100f;
    private Dictionary<MapPolyNexus, Waypoint> _nexusWps;
    private Dictionary<MapPolygonEdge, List<Waypoint>> _edgeWps;
    private Dictionary<MapPolygon, List<List<Waypoint>>> _interiorWps;
    
    
    public override GenReport Generate(GenWriteKey key)
    {
        _id = key.Data.IdDispenser;
        var genReport = new GenReport("Nav");
        
        genReport.StartSection();
        GenerateEdgeWps(key);
        genReport.StopSection("edge wps", "");
        
        genReport.StartSection();
        GenerateNexusWps(key);
        genReport.StopSection("nexus wps", "");

        
        genReport.StartSection();
        GenerateInnerBoundaryAndInteriorWps(key);
        genReport.StopSection("inner wps", "");

        genReport.StartSection();
        Connect(key);
        genReport.StopSection("connect wps", "");

        
        var wps = _nexusWps.Values
            .Union(_edgeWps.SelectMany(kvp => kvp.Value))
            .Union(_interiorWps.SelectMany(kvp => kvp.Value.SelectMany(v => v)))
            .ToDictionary(wp => wp.Id, wp => wp);
        
        
        genReport.StartSection();
        SetLandWaypointProperties(key, wps.Values);
        genReport.StopSection("set wp properties", "");

        
        
        var newNav = TacticalWaypoints.Create(key, wps);
        
        return genReport;
    }

    private void GenerateNexusWps(GenWriteKey key)
    {
        var res = new Dictionary<MapPolyNexus, Waypoint>();
        foreach (var nexus in key.Data.GetAll<MapPolyNexus>())
        {
            var river = nexus.IsRiverNexus(key.Data);
            var incidentPolys = nexus.IncidentPolys.Items(key.Data);
            var numIncident = incidentPolys.Count();
            var coast 
                = incidentPolys.Any(p => p.IsWater())
                && incidentPolys.Any(p => p.IsLand);
            var p0 = numIncident > 0 ? incidentPolys.ElementAt(0) : null;
            var p1 = numIncident > 1 ? incidentPolys.ElementAt(1) : null;
            var p2 = numIncident > 2 ? incidentPolys.ElementAt(2) : null;
            var p3 = numIncident > 3 ? incidentPolys.ElementAt(3) : null;
            if (coast)
            {
                var firstSeaPoly = incidentPolys.First(p => p.IsWater());
                var sea = key.Data.Planet.PolygonAux.LandSea.SeaDic[firstSeaPoly].Id;
                
                if (river)
                {
                    var wp = new RiverMouthWaypoint(key, _id.TakeId(),
                        nexus.Point, sea, p0, p1, p2, p3);
                    res.Add(nexus, wp);
                }
                else
                {
                    var wp = new CoastWaypoint(key, sea, false, _id.TakeId(),
                        nexus.Point, p0, p1, p2, p3);
                    res.Add(nexus, wp);
                }
            }
            else if (river)
            {
                var wp = new RiverWaypoint(key, _id.TakeId(), nexus.Point, p0, p1, p2, p3);
                res.Add(nexus, wp);
            }
        }

        _nexusWps = res;
    }

    private void GenerateEdgeWps(GenWriteKey key)
    {
        var lists = key.Data.GetAll<MapPolygonEdge>()
            .AsParallel()
            .Select(e => new KeyValuePair<MapPolygonEdge, List<Waypoint>>(e, handle(e)))
            .Where(kvp => kvp.Value != null);
        _edgeWps = new Dictionary<MapPolygonEdge, List<Waypoint>>();
        _edgeWps.AddRange(lists);
            
        List<Waypoint> handle(MapPolygonEdge edge)
        {
            var river = edge.IsRiver();
            var hi = edge.HighPoly.Entity(key.Data);
            var lo = edge.LowPoly.Entity(key.Data);
            var coast = (hi.IsWater() || lo.IsWater())
                        && (hi.IsLand || lo.IsLand);
            if (river == false && coast == false) return null;
            
            var segs = edge.GetSegsAbs(key.Data);
            var res = new List<Waypoint>();

            int sea = -1;
            if (coast)
            {
                sea = lo.IsWater()
                    ? key.Data.Planet.PolygonAux.LandSea.SeaDic[lo].Id
                    : key.Data.Planet.PolygonAux.LandSea.SeaDic[hi].Id;
            }

            var edgeLength = edge.GetLength(key.Data);
            var numEdgeWps = Mathf.FloorToInt(edgeLength / _waypointDist);
            for (var i = 1; i <= numEdgeWps; i++)
            {
                var ratio = (float)i / (numEdgeWps + 1);
                var pos = segs.GetPointAlong(ratio);
                makeWp(pos);
            }

            void makeWp(Vector2 pos)
            {
                if (river)
                {
                    var wp = new RiverWaypoint(key, _id.TakeId(), pos, hi, lo);
                    res.Add(wp);
                }
                else
                {
                    var wp = new CoastWaypoint(key, sea, false,
                        _id.TakeId(), pos, hi, lo);
                    res.Add(wp);
                }
            }

            return res;
        }
    }

    private void GenerateInnerBoundaryAndInteriorWps(GenWriteKey key)
    {
        _interiorWps = new Dictionary<MapPolygon, List<List<Waypoint>>>();
        var lists = key.Data.GetAll<MapPolygon>()
            .AsParallel()
            .Select(p => new KeyValuePair<MapPolygon,List<List<Waypoint>>>(p, handle(p)));
        _interiorWps.AddRange(lists);

        List<List<Waypoint>> handle(MapPolygon poly)
        {
            var res = new List<List<Waypoint>>();
            if (poly.IsWater())
            {
                var wp = new SeaWaypoint(key, _id.TakeId(), poly.Center, poly);
                res.Add(new List<Waypoint>{wp});
                return res;
            }

            var currRing = new List<Waypoint>();
            var border = poly.GetOrderedBoundaryPoints(key.Data)
                .Select(v => v + poly.Center)
                .Distinct()
                .ToArray();

            var inner = addRing(poly, border, 
                wp =>
                {
                    currRing.Add(wp);
                }, 
                -_waypointDist / 4f);
            while (inner != null)
            {
                res.Add(currRing);
                currRing = new List<Waypoint>();
                var innerInner = addRing(poly, inner,
                    wp =>
                    {
                        currRing.Add(wp);
                    }, -_waypointDist / 2f);
                inner = innerInner;
            }

            return res;
        }

        Vector2[] addRing(MapPolygon poly, Vector2[] border, Action<Waypoint> add, float shrink)
        {
            var polyInnerBorders = Geometry2D.OffsetPolygon(border, shrink);
            if (polyInnerBorders.Count == 0) return null;
            var polyInnerBorder = polyInnerBorders
                .MaxBy(b => b.Count());
            var innerLength = 0f;
            for (var i = 0; i < polyInnerBorder.Length; i++)
            {
                innerLength += polyInnerBorder[i].DistanceTo(polyInnerBorder.Modulo(i + 1));
            }

            var numInnerBoundaryPoints = Mathf.CeilToInt(innerLength / _waypointDist);
            var newWpPoses = new List<Vector2>();
            for (var i = 0; i < numInnerBoundaryPoints; i++)
            {
                var ratio = (float)i / (numInnerBoundaryPoints);
                var pos = polyInnerBorder.GetPointAlongCircle((v, w) => w - v, ratio);
                var wp = new InlandWaypoint(key, _id.TakeId(), 
                    pos,
                    poly);
                newWpPoses.Add(wp.Pos);
                add(wp);
            }

            return newWpPoses.ToArray();
        }
    }
    


    private void Connect(GenWriteKey key)
    {
        var dic = new Dictionary<MapPolygon, HashSet<Waypoint>>();
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            var nexusWps = poly
                .GetNexi(key.Data)
                .Where(n => _nexusWps.ContainsKey(n))
                .Select(n => _nexusWps[n]);
            var edgeWps = poly
                .GetEdges(key.Data)
                .Where(n => _edgeWps.ContainsKey(n))
                .SelectMany(n => _edgeWps[n]);
            
            var interiorWps = _interiorWps[poly]
                .SelectMany(l => l);
            var all = nexusWps.Union(edgeWps).Union(interiorWps);
            dic.Add(poly, all.ToHashSet());
        }

        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            if (poly.IsLand)
            {
                doLandLink(poly, poly);
                var lastRing = _interiorWps[poly].Last();

                bool isClose(Vector2 p, Vector2 a, Vector2 b)
                {
                    if (p == b || p == a) return false;
                    var offsetA = key.Data.Planet.GetOffsetTo(p, a);
                    var offsetB = key.Data.Planet.GetOffsetTo(p, b);
                    return offsetA.AngleTo(offsetB) >= Mathf.Pi * .8f;
                }
                
                for (var i = 0; i < lastRing.Count; i++)
                {
                    for (var j = i + 1; j < lastRing.Count; j++)
                    {
                        var a = lastRing[i];
                        var b = lastRing[j];
                        if (lastRing.Any(p => isClose(p.Pos, a.Pos, b.Pos) == false))
                        {
                            link(a, b);
                        }
                    }
                }
            }
            foreach (var nPoly in poly.Neighbors.Items(key.Data))
            {
                if (nPoly.Id < poly.Id) continue;
                if (poly.IsWater() && nPoly.IsWater())
                    doSeaLink(poly, nPoly);
                else if(poly.IsWater() && nPoly.IsLand)
                    doCoastLink(poly, nPoly);
                else if(nPoly.IsWater() && poly.IsLand)
                    doCoastLink(nPoly, poly);
                else 
                    doLandLink(poly, nPoly);
            }
        }
        void link(Waypoint w, Waypoint v)
        {
            w.Neighbors.Add(v.Id);
            v.Neighbors.Add(w.Id);
        }

        void doCoastLink(MapPolygon seaPoly, MapPolygon coastPoly)
        {
            var edge = seaPoly.GetEdge(coastPoly, key.Data);
            var edgeWps = _edgeWps[edge];
                
            var seaWp = _interiorWps[seaPoly].First().First();
            foreach (var edgeWp in edgeWps)
            {
                link(edgeWp, seaWp);
            }

            var hiN = edge.HiNexus.Entity(key.Data);
            if (_nexusWps.ContainsKey(hiN))
            {
                link(_nexusWps[hiN], seaWp);
            }
                
            var loN = edge.HiNexus.Entity(key.Data);
            if (_nexusWps.ContainsKey(loN))
            {
                link(_nexusWps[loN], seaWp);
            }
        }
            
        void doSeaLink(MapPolygon p1, MapPolygon p2)
        {
            link(_interiorWps[p1].First().First(), _interiorWps[p2].First().First());
        }

        void doLandLink(MapPolygon p1, MapPolygon p2)
        {
            if (p1 != p2 && p1.GetEdge(p2, key.Data).IsRiver())
            {
                return;
            }
            var all1 = dic[p1];
            var all2 = dic[p2];
            foreach (var wp1 in all1)
            {
                foreach (var wp2 in all2)
                {
                    if (wp1 == wp2) continue;
                    var offset = key.Data.Planet.GetOffsetTo(wp1.Pos, wp2.Pos);
                    if (offset.Length() < _waypointDist)
                    {
                        link(wp1, wp2);
                    }
                }
            }
        }
    }
    
    private void SetLandWaypointProperties(GenWriteKey key, IEnumerable<Waypoint> waypoints)
    {
        var res = Parallel.ForEach(waypoints, handle);
        if (res.IsCompleted == false) throw new Exception();
        void handle(Waypoint waypoint)
        {
            if (waypoint is ILandWaypoint n == false) return;
            var pos = key.Data.Planet.ClampPosition(waypoint.Pos);

            var rTotal = 0f;
            var assoc = waypoint
                .AssocPolys(key.Data)
                .Where(p => p.IsLand);
            var numAssoc = assoc.Count();
            foreach (var poly in assoc)
            {
                var offset = poly.GetOffsetTo(waypoint.Pos, key.Data);
                var roughSample= poly.Tris.Tris
                    .Where(t => t.Landform(key.Data).IsLand)
                    .OrderBy(t => t.GetCentroid().DistanceTo(offset))
                    .Take(4);
                if (roughSample.Count() == 0) continue;
                var totalArea = roughSample.Sum(s => s.GetArea());
                var totalRoughness = roughSample.Sum(s => s.Landform(key.Data).MinRoughness * s.GetArea());

                var roughVal = totalRoughness / totalArea;
                
                if (float.IsNaN(roughVal) == false)
                {
                    rTotal += roughVal;
                }
            }
            
            n.SetRoughness(rTotal / numAssoc, key);
        }
    }
}