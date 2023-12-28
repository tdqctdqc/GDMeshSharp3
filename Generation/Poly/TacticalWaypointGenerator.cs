using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class TacticalWaypointGenerator : Generator
{
    private IdDispenser _id;
    private float _edgeWaypointDist = 25f, 
        _innerWaypointDist = 25f;
    private Dictionary<MapPolyNexus, Waypoint> _nexusWps;
    private Dictionary<MapPolygonEdge, List<Waypoint>> _edgeWps;
    private Dictionary<Vector2I, List<Waypoint>> _riverBankWps;
    private HashSet<Vector2> _takenPoses;
    private Dictionary<int, Waypoint> _byId;
    private Dictionary<Vector2, Tuple<MapPolygon, Waypoint>> _innerWps;
    private Dictionary<MapPolygon, Waypoint> _seaWps;
    private HashSet<Vector2I> _shortEdgeConnects;
    private ConcurrentDictionary<MapPolygon, HashSet<Vector2>> _landHash;
    public override GenReport Generate(GenWriteKey key)
    {
        _id = key.Data.IdDispenser;
        _takenPoses = new HashSet<Vector2>();
        var genReport = new GenReport("Nav");
        
        genReport.StartSection();
        GenerateEdgeWps(key);
        genReport.StopSection("edge wps", "");
        
        genReport.StartSection();
        GenerateRiverBankWaypoints(key);
        genReport.StopSection("river bank wps", "");
        
        genReport.StartSection();
        GenerateNexusWps(key);
        genReport.StopSection("nexus wps", "");
        _takenPoses.AddRange(_edgeWps.SelectMany(kvp => kvp.Value.Select(wp => wp.Pos)));
        _takenPoses.AddRange(_nexusWps.Values.Select(wp => wp.Pos));
        
        genReport.StartSection();
        GenerateInnerWaypoints(key);
        genReport.StopSection("generate inner wps");
        
        
        _byId = _nexusWps.Values
            .Union(_edgeWps.SelectMany(kvp => kvp.Value))
            .Union(_innerWps.Select(kvp => kvp.Value.Item2))
            .Union(_seaWps.Values)
            .Union(_riverBankWps.SelectMany(kvp => kvp.Value))
            .ToDictionary(wp => wp.Id, wp => wp);
        
        
        genReport.StartSection();
        Connect(key);
        genReport.StopSection("connect wps", "");
        
        genReport.StartSection();
        DoShortEdgeWps(key);
        genReport.StopSection("short edge wps", "");

        
        
        genReport.StartSection();
        SetLandWaypointProperties(key, _byId.Values);
        genReport.StopSection("set wp properties", "");

        
        var newNav = TacticalWaypoints.Create(key, _byId);
        MakePolyCenters(key);
        MakePolyCenterPaths(key);
        
        key.Data.Notices.MadeWaypoints.Invoke();
        
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
                wp.MakeBridgeable(key);
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
            var numEdgeWps = Mathf.FloorToInt(edgeLength / _edgeWaypointDist);
            for (var i = 1; i <= numEdgeWps; i++)
            {
                var ratio = (float)i / (numEdgeWps + 1);
                var pos = segs.GetPointAlong(ratio);
                makeWp(pos);
            }

            if (river)
            {
                var hiBank = new List<Waypoint>();
                var loBank = new List<Waypoint>();
                for (var i = 1; i < res.Count - 1; i++)
                {
                    if (i % 2 == 0) continue;
                    ((RiverWaypoint)res[i]).MakeBridgeable(key);
                }
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

    private void GenerateRiverBankWaypoints(GenWriteKey key)
    {
        _riverBankWps = new Dictionary<Vector2I, List<Waypoint>>();
        foreach (var kvp in _edgeWps)
        {
            var edge = kvp.Key;
            if (edge.IsRiver() == false) continue;
            var riverWps = kvp.Value;
            var hi = edge.HighPoly.Entity(key.Data);
            var hiPos = hi.Center;
            var lo = edge.LowPoly.Entity(key.Data);
            var loPos = lo.Center;
            var hiWps = new List<Waypoint>();
            var loWps = new List<Waypoint>();
            for (var i = 0; i < riverWps.Count; i++)
            {
                var riverWp = riverWps[i];
                var hiOffset = riverWp.Pos
                    .GetOffsetTo(hiPos, key.Data).Normalized() * _edgeWaypointDist / 2f;
                var loOffset = riverWp.Pos
                    .GetOffsetTo(loPos, key.Data).Normalized() * _edgeWaypointDist / 2f;

                var hiWpPos = (riverWp.Pos + hiOffset).ClampPosition(key.Data);
                var loWpPos = (riverWp.Pos + loOffset).ClampPosition(key.Data);

                if (hi.PointInPolyAbs(hiWpPos, key.Data))
                {
                    hiWps.Add(new InlandWaypoint(key, _id.TakeId(), hiWpPos, hi));
                }
                else
                {
                    throw new Exception();
                }
                
                if (lo.PointInPolyAbs(loWpPos, key.Data))
                {
                    loWps.Add(new InlandWaypoint(key, _id.TakeId(), loWpPos, lo));
                }
                else
                {
                    throw new Exception();
                }
            }

            _riverBankWps.Add(new Vector2I(edge.Id, hi.Id), hiWps);
            _riverBankWps.Add(new Vector2I(edge.Id, lo.Id), loWps);
        }
    }
    private void GenerateInnerWaypoints(GenWriteKey key)
    {
        _innerWps = new Dictionary<Vector2, Tuple<MapPolygon, Waypoint>>();
        _seaWps = new Dictionary<MapPolygon, Waypoint>();

        _landHash = new ConcurrentDictionary<MapPolygon, HashSet<Vector2>>();
        var polys = key.Data.GetAll<MapPolygon>();
        var landPolys = polys
            .Where(p => p.IsLand);
        foreach (var waterPoly in polys.Where(p => p.IsWater()))
        {
            var wp = new SeaWaypoint(key, _id.TakeId(),
                waterPoly.Center, waterPoly);
            _seaWps.Add(waterPoly, wp);
            _takenPoses.Add(waterPoly.Center);
        }

        Parallel.ForEach(landPolys, poly =>
        {
            var hash = new HashSet<Vector2>();

            var edgeWps = poly.GetEdges(key.Data)
                .Where(e => _edgeWps.ContainsKey(e))
                .SelectMany(e => _edgeWps[e]);
            var nexusWps = poly.GetNexi(key.Data)
                .Where(_nexusWps.ContainsKey)
                .Select(n => _nexusWps[n]);
            var riverBankWps = poly.GetEdges(key.Data)
                .Where(e => e.IsRiver())
                .SelectMany(e => _riverBankWps[new Vector2I(e.Id, poly.Id)]);

            var suppressingWps = edgeWps
                .Union(nexusWps)
                .Union(riverBankWps)
                .ToArray();
            
            var boundaryPs = poly.GetOrderedBoundaryPoints(key.Data);
            var minX = boundaryPs.Min(v => v.X) + poly.Center.X;
            var minXIndex = Mathf.FloorToInt(minX / _innerWaypointDist);

            var maxX = boundaryPs.Max(v => v.X) + poly.Center.X;
            var maxXIndex = Mathf.CeilToInt(maxX / _innerWaypointDist);

            var minY = boundaryPs.Min(v => v.Y) + poly.Center.Y;
            var minYIndex = Mathf.FloorToInt(minY / _innerWaypointDist);
            
            var maxY = boundaryPs.Max(v => v.Y) + poly.Center.Y;
            var maxYIndex = Mathf.CeilToInt(maxY / _innerWaypointDist);

            for (int i = minXIndex; i <= maxXIndex; i++)
            {
                for (int j = minYIndex; j <= maxYIndex; j++)
                {
                    var pos = new Vector2(i, j) * _innerWaypointDist;
                    pos = pos.ClampPosition(key.Data);
                    var offset = poly.Center.GetOffsetTo(pos, key.Data);
                    if (suppressingWps.Any(s => 
                            s.Pos.GetOffsetTo(pos, key.Data).Length() 
                                < _edgeWaypointDist * .5f))
                    {
                        continue;
                    }
                    if (Geometry2D.IsPointInPolygon(offset, boundaryPs) == false) continue;
                    hash.Add(pos);
                }
            }
            _landHash.TryAdd(poly, hash);
        });
        foreach (var kvp in _landHash)
        {
            var poly = kvp.Key;
            foreach (var pos in kvp.Value)
            {
                if (_takenPoses.Contains(pos)) continue;
                _takenPoses.Add(pos);
                var wp = new InlandWaypoint(key, _id.TakeId(), pos, poly);
                _innerWps.Add(pos, new(poly, wp));
            }
        }
    }


    private void DoShortEdgeWps(GenWriteKey key)
    {
        foreach (var kvp in _landHash)
        {
            var poly = kvp.Key;
            if (poly.IsWater()) continue;
            var wps = kvp.Value;
            foreach (var nPoly in poly.Neighbors.Items(key.Data))
            {
                if (nPoly.Id > poly.Id) continue;
                if (nPoly.IsWater()) continue;
                var edge = poly.GetEdge(nPoly, key.Data);
                if (edge.IsRiver()) continue;
                var nWps = _landHash[nPoly];
                if (noLink())
                {
                    var midPoint = edge.GetSegsRel(poly, key.Data)
                        .Segments.GetPointAlong(.5f) + poly.Center;
                    midPoint = midPoint.ClampPosition(key.Data);
                    var midWp = new InlandWaypoint(key, _id.TakeId(), midPoint,
                        poly, nPoly);
                    join(midWp, wps);
                    join(midWp, nWps);
                    _byId.Add(midWp.Id, midWp);
                }

                bool noLink()
                {
                    return wps.Any(wp =>
                        _innerWps[wp].Item2.Neighbors.Any(n => 
                            nWps.Contains(_byId[n].Pos))) == false;
                }

                void join(Waypoint midWp, IEnumerable<Vector2> candWps)
                {
                    var close = candWps.Select(v => _innerWps[v].Item2)
                        .Where(wp => midWp.Pos.GetOffsetTo(wp.Pos, key.Data).Length() < _innerWaypointDist * 2f);
                    if (close.Count() == 0)
                    {
                        var closest = candWps
                            .Select(v => _innerWps[v].Item2)
                            .MinBy(wp => midWp.Pos.GetOffsetTo(wp.Pos, key.Data).Length());
                        midWp.Neighbors.Add(closest.Id);
                        closest.Neighbors.Add(midWp.Id);
                    }
                    foreach (var closeWp in close)
                    {
                        midWp.Neighbors.Add(closeWp.Id);
                        closeWp.Neighbors.Add(midWp.Id);
                    }
                }
            }
        }
    }

    private void Connect(GenWriteKey key)
    {
        var links = new ConcurrentBag<Vector2I>();
        var polys = key.Data.GetAll<MapPolygon>();
        var innersByPoly = new Dictionary<MapPolygon, List<Waypoint>>();
        foreach (var kvp in _innerWps)
        {
            var poly = kvp.Value.Item1;
            innersByPoly.AddOrUpdate(poly, kvp.Value.Item2);
        }
        Parallel.ForEach(polys, poly =>
        {
            foreach (var nPoly in poly.Neighbors.Items(key.Data))
            {
                if (nPoly.Id < poly.Id) continue;
                var edge = nPoly.GetEdge(poly, key.Data);
                
                if (poly.IsWater() && nPoly.IsWater())
                    doSeaLink(poly, nPoly);
                else if(poly.IsWater() && nPoly.IsLand)
                    doCoastLink(poly, nPoly);
                else if(nPoly.IsWater() && poly.IsLand)
                    doCoastLink(nPoly, poly);
                else if (edge.IsRiver())
                {
                    doEdgeLinks(edge);
                    // doInnerToEdgeLinks(edge);
                    doRiverBankLinks(edge);
                }
            }
        });
        foreach (var kvp in _nexusWps)
        {
            var nexus = kvp.Key;
            var nexusWp = kvp.Value;
            var inners = nexus.IncidentPolys.Items(key.Data)
                .Where(p => innersByPoly.ContainsKey(p))
                .SelectMany(p => innersByPoly[p]);
            var hiBanks = nexus.IncidentEdges.Items(key.Data)
                .Where(e => e.IsRiver())
                .SelectMany(e => _riverBankWps[new Vector2I(e.Id, e.HighPoly.RefId)]);
            var loBanks = nexus.IncidentEdges.Items(key.Data)
                .Where(e => e.IsRiver())
                .SelectMany(e => _riverBankWps[new Vector2I(e.Id, e.LowPoly.RefId)]);
            var relevant = inners.Union(hiBanks).Union(loBanks);
            foreach (var wp in relevant)
            {
                if (wp.Pos.GetOffsetTo(nexusWp.Pos, key.Data).Length() < _edgeWaypointDist * 2f)
                {
                    link(nexusWp, wp);
                }
            }
        }
        

        Parallel.ForEach(_innerWps, doLandWp);
        foreach (var ids in links)
        {
            var w = _byId[ids.X];
            var v = _byId[ids.Y];
            w.Neighbors.Add(v.Id);
            v.Neighbors.Add(w.Id);
        }
        void link(Waypoint w, Waypoint v)
        {
            links.Add(new Vector2I(w.Id, v.Id));
        }

    
        void doCoastLink(MapPolygon seaPoly, MapPolygon coastPoly)
        {
            var edge = seaPoly.GetEdge(coastPoly, key.Data);
            var seaWp = _seaWps[seaPoly];
            
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
            doEdgeLinks(edge);
            doInnerToEdgeLinks(edge);
        }
            
        void doSeaLink(MapPolygon p1, MapPolygon p2)
        {
            link(_seaWps[p1], _seaWps[p2]);
        }

        void doEdgeLinks(MapPolygonEdge edge)
        {
            var wps = _edgeWps[edge];
            var n1 = _nexusWps[edge.HiNexus.Entity(key.Data)];
            var n2 = _nexusWps[edge.LoNexus.Entity(key.Data)];

            if (wps.Count == 0)
            {
                link(n1, n2);
                return;
            }

            var closeTo1 = wps.MinBy(wp => n1.Pos.GetOffsetTo(wp.Pos, key.Data).Length());
            link(n1, closeTo1);
            var closeTo2 = wps.MinBy(wp => n2.Pos.GetOffsetTo(wp.Pos, key.Data).Length());
            link(n2, closeTo2);
            for (var i = 0; i < wps.Count - 1; i++)
            {
                link(wps[i], wps[i + 1]);
            }
        }

        void doRiverBankLinks(MapPolygonEdge edge)
        {
            if (edge.IsRiver() == false) return;
            var hiBankWps = _riverBankWps[new Vector2I(edge.Id, edge.HighPoly.RefId)];
            var loBankWps = _riverBankWps[new Vector2I(edge.Id, edge.LowPoly.RefId)];
            for (var i = 0; i < hiBankWps.Count; i++)
            {
                link(hiBankWps[i], loBankWps[i]);
            }
            
            var riverWps = _edgeWps[edge];
            var hiPoly = edge.HighPoly.Entity(key.Data);
            doBankWps(hiPoly);
            var loPoly = edge.LowPoly.Entity(key.Data);
            doBankWps(loPoly);


            var iRiverEdges = edge.GetIncidentEdges(key.Data)
                .Where(e => e.IsRiver());
            foreach (var iRiverEdge in iRiverEdges)
            {
                if (iRiverEdge.Id > edge.Id) continue;
                var poly = iRiverEdge.EdgeToPoly(hiPoly)
                    ? hiPoly
                    : loPoly;
                var iRiverBankWps = _riverBankWps[new Vector2I(iRiverEdge.Id, poly.Id)];
                var bankWps = _riverBankWps[new Vector2I(edge.Id, poly.Id)];
                if (iRiverBankWps.Count == 0 || bankWps.Count == 0) continue;
                Waypoint from = null;
                Waypoint to = null;
                float dist = Mathf.Inf;
                check(bankWps.First(), iRiverBankWps.First());
                check(bankWps.Last(), iRiverBankWps.First());
                check(bankWps.First(), iRiverBankWps.Last());
                check(bankWps.Last(), iRiverBankWps.Last());
                
                link(from, to);

                void check(Waypoint w, Waypoint v)
                {
                    var offset = w.Pos.GetOffsetTo(v.Pos, key.Data).Length();
                    if (offset < dist)
                    {
                        dist = offset;
                        from = w;
                        to = v;
                    }
                }

            }

            void doBankWps(MapPolygon poly)
            {
                var bankWps = _riverBankWps[new Vector2I(edge.Id, poly.Id)];
                var innerWps = innersByPoly[poly];
                for (var i = 0; i < bankWps.Count - 1; i++)
                {
                    link(bankWps[i], bankWps[i + 1]);
                }
                foreach (var bankWp in bankWps)
                {
                    var closeRWps = riverWps.Where(rWp =>
                        rWp.Pos.GetOffsetTo(bankWp.Pos, key.Data).Length() < _edgeWaypointDist);
                    foreach (var closeRWp in closeRWps)
                    {
                        link(bankWp, closeRWp);
                    }
                    var closeInners = innerWps.Where(iWp =>
                        iWp.Pos.GetOffsetTo(bankWp.Pos, key.Data).Length() < _edgeWaypointDist * 2f);
                    foreach (var closeInnerWp in closeInners)
                    {
                        link(bankWp, closeInnerWp);
                    }
                }
            }
            
            
            
        }
        void doInnerToEdgeLinks(MapPolygonEdge edge)
        {
            var dist = _innerWaypointDist * 2f;
            var n1 = _nexusWps[edge.HiNexus.Entity(key.Data)];
            var n2 = _nexusWps[edge.LoNexus.Entity(key.Data)];
            var es = _edgeWps[edge];
            checkClose(n1);
            checkClose(n2);
            foreach (var e in es)
            {
                checkClose(e);
            }
            void checkClose(Waypoint wp)
            {
                var minX = Mathf.FloorToInt((wp.Pos.X - dist) / _innerWaypointDist);
                var maxX = Mathf.CeilToInt((wp.Pos.X + dist) / _innerWaypointDist);
                var minY = Mathf.FloorToInt((wp.Pos.Y - dist) / _innerWaypointDist);
                var maxY = Mathf.CeilToInt((wp.Pos.Y + dist) / _innerWaypointDist);
                for (int i = minX; i <= maxX; i++)
                {
                    for (int j = minY; j <= maxY; j++)
                    {
                        var pos = new Vector2(i, j) * _innerWaypointDist;
                        pos = pos.ClampPosition(key.Data);
                        if (_innerWps.TryGetValue(pos, out var lwp)
                            && edge.EdgeToPoly(lwp.Item1)
                            && wp.Pos.GetOffsetTo(lwp.Item2.Pos, key.Data).Length() < dist)
                        {
                            link(wp, lwp.Item2);
                        }
                    }
                }
            }
        }

        void doLandWp(KeyValuePair<Vector2, Tuple<MapPolygon, Waypoint>> kvp)
        {
            var pos = kvp.Key;
            var wp = kvp.Value.Item2;
            var poly = kvp.Value.Item1;
            
            doLandLink(1, 0);
            // doLandLink(2, 1);
            doLandLink(1, 1);
            // doLandLink(1, 2);
            
            doLandLink(0, 1);
            // doLandLink(-1, 2);
            doLandLink(-1, 1);
            // doLandLink(-2, 1);
            
            
            void doLandLink(int xOffset, int yOffset)
            {
                var nPos = pos + new Vector2(xOffset, yOffset) * _innerWaypointDist;
                nPos = nPos.ClampPosition(key.Data);
                
                if (_innerWps.ContainsKey(nPos))
                {
                    var nPoly = _innerWps[nPos].Item1;
                    if (nPoly.Neighbors.Contains(poly)
                        && nPoly.GetEdge(poly, key.Data).IsRiver())
                    {
                    }
                    else
                    {
                        link(wp, _innerWps[nPos].Item2);
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
            var pos = waypoint.Pos.ClampPosition(key.Data);
            var rTotal = 0f;
            var assoc = waypoint
                .AssocPolys(key.Data)
                .Where(p => p.IsLand);
            int numAssoc = 0;
            foreach (var poly in assoc)
            {
                numAssoc++;
                var offset = poly.GetOffsetTo(waypoint.Pos, key.Data);
                var roughSample= poly.Tris.Tris
                    .Where(t => t.Landform(key.Data).IsLand)
                    .OrderBy(t => t.GetCentroid().DistanceTo(offset))
                    .Take(4).ToArray();
                if (roughSample.Length == 0) continue;
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

    private void MakePolyCenters(GenWriteKey key)
    {
        var tacWps = key.Data.Military.TacticalWaypoints;
        var polys = key.Data.GetAll<MapPolygon>();
        foreach (var p in polys)
        {
            var center = p.GetAssocTacWaypoints(key.Data)
                .MinBy(wp => wp.Pos.GetOffsetTo(p.Center, key.Data).Length());
            tacWps.PolyCenterWpIds[p.Id] = center.Id;
        }
    }
    private void MakePolyCenterPaths(GenWriteKey key)
    {
        var tacWps = key.Data.Military.TacticalWaypoints;
        var polys = key.Data.GetAll<MapPolygon>();
        foreach (var p in polys)
        {
            foreach (var np in p.Neighbors.Items(key.Data))
            {
                var idKey = new Vector2I(p.Id, np.Id);
                var inverse = new Vector2I(np.Id, p.Id);
                if (tacWps.PolyCenterPaths.TryGetValue(inverse, out var oldPath))
                {
                    tacWps.PolyCenterPaths.Add(idKey, ((IEnumerable<int>)oldPath).Reverse().ToList());
                }
                else
                {
                    var path = PathFinder.FindNavPathBetweenPolygons(p, np, key.Data);
                    
                    tacWps.PolyCenterPaths.Add(idKey, 
                        path.Select(wp => wp.Id).ToList());
                }
            }
        }
    }
}