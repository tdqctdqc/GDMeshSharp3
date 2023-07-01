using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class PolyTriGenerator : Generator
{
    private GenData _data;
    private IdDispenser _idd;
    public PolyTriGenerator()
    {
    }
    public override GenReport Generate(GenWriteKey key)
    {
        _idd = key.IdDispenser;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        var polys = _data.Planet.Polygons.Entities;
        
        report.StartSection();
        var riverData = new RiverPolyTriGen().DoRivers(key);
        report.StopSection("Finding rivers");
        
        report.StartSection();
        Parallel.ForEach(polys, p =>
        {
            BuildTris(p, riverData, key);
        });
        report.StopSection("Building poly terrain tris");
        
        report.StartSection();
        var edgeTris = new ConcurrentBag<(PolyTriPosition[], PolyTriPosition[])>();
        Parallel.ForEach(_data.Planet.PolyEdges.Entities, p => CollectEdgeTris(edgeTris, p, key));
        Parallel.ForEach(_data.Planet.Polygons.Entities, p => p.Tris.SetNeighbors(p, key)); 
        report.StopSection("making poly tri paths");
        
        _data.Notices.SetPolyShapes.Invoke();
        
        report.StartSection();
        Postprocess(key);
        report.StopSection("postprocessing polytris");

        report.StartSection();
        Parallel.ForEach(_data.Planet.Polygons.Entities, p => p.SetTerrainStats(key)); 
        report.StopSection("setting terrain stats");

        return report;
    }

    private void CheckJoins(GenWriteKey key)
    {
        foreach (var poly in key.Data.Planet.Polygons.Entities)
        {
            foreach (var n in poly.Neighbors)
            {
                if(n.Id < poly.Id) continue;
                var edge = poly.GetEdge(n, key.Data);
                var hi = edge.HighPoly.Entity();
                var lo = edge.LowPoly.Entity();
                var hiSegs = edge.HighSegsRel().Segments;
                var loSegs = edge.LowSegsRel().Segments;
                
                for (var i = 0; i < hiSegs.Count; i++)
                {
                    var loI = hiSegs.Count - i - 1;
                    var hiAbs = hiSegs[i].From + hi.Center;
                    var loAbs = loSegs[loI].To + lo.Center;
                    if (hiAbs.DistanceTo(loAbs) > 0f)
                    { 
                        // GD.Print($"{hi.Center} {lo.Center}");
                        GD.Print($"{hiAbs} {loAbs}");
                        GD.Print($"{hiAbs.X - (int)(hiAbs.X)} {hiAbs.Y - (int)(hiAbs.Y)}");
                        GD.Print($"{loAbs.X - (int)(loAbs.X)} {loAbs.Y - (int)(loAbs.Y)}");

                        
                        
                        // GD.Print($"{hiSegs[i].From} {loSegs[loI].To}");
                        // GD.Print($"{hiAbs.X - loAbs.X} {hiAbs.Y - loAbs.Y}");
                        // GD.Print(hiAbs - loAbs);

                        // var hiX = hiAbs.X.ToString().ToFloat();
                        // var hiY = hiAbs.Y.ToString().ToFloat();
                        // var loX = loAbs.X.ToString().ToFloat();
                        // var loY = loAbs.Y.ToString().ToFloat();
                        // var hiStringed = new Vector2(hiX, hiY);
                        // var loStringed = new Vector2(loX, loY);
                        // GD.Print($"{hiStringed} {loStringed} {hiStringed == loStringed}");
                        throw new Exception($"{hiAbs} {loAbs} {hiAbs.DistanceTo(loAbs)}");
                    }
                }
            }
        }
    }
    private void BuildTris(MapPolygon poly, TempRiverData rd, GenWriteKey key)
    {
        List<PolyTri> tris;
        if (poly.IsWater())
        {
            tris = DoSeaPoly(poly, key);
        }
        else if (poly.GetNexi(key.Data).Any(n => n.IsRiverNexus()))
        {
            tris = NewRiverTriGen.DoPoly(poly, key.Data, rd, key);
        }
        else
        {
            tris = DoLandPolyNoRivers(poly, key);
        }
        
        var polyTerrainTris = PolyTris.Create(tris,  key);
        if (polyTerrainTris == null) throw new Exception();
        poly.SetTerrainTris(polyTerrainTris, key);
    }
    
    private List<PolyTri> DoSeaPoly(MapPolygon poly, GenWriteKey key)
    {
        var tris = new List<PolyTri>();
        var boundaryPs = poly.GetOrderedBoundaryPoints(_data);
        var triPIndices = Geometry2D.TriangulatePolygon(boundaryPs);
        for (var i = 0; i < triPIndices.Length; i+=3)
        {
            var a = boundaryPs[triPIndices[i]];
            var b = boundaryPs[triPIndices[i+1]];
            var c = boundaryPs[triPIndices[i+2]];
            tris.Add(PolyTri.Construct(poly.Id, a,b,c,LandformManager.Sea,
                VegetationManager.Barren));
        }
        return tris;
    }
    
    private List<PolyTri> DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderPs = poly.GetOrderedBoundaryPoints(_data);
        List<PolyTri> tris = borderPs.PolyTriangulate(key.Data, poly);

        return tris;
    }

    

    private void CollectEdgeTris(ConcurrentBag<(PolyTriPosition[], PolyTriPosition[])> edgeTris,
        MapPolygonEdge edge, GenWriteKey key)
    {
        var lo = edge.LowPoly.Entity();
        var hi = edge.HighPoly.Entity();

        var loSegs = lo.GetBorder(hi.Id).Segments;
        var hiSegs = hi.GetBorder(lo.Id).Segments;
        
        var loEdgeTris = lo.Tris.Tris
            .Where(t => t.AnyPoint(p => loSegs.Any(ls => ls.ContainsVertex(p)))).ToArray();
        var hiEdgeTris = hi.Tris.Tris
            .Where(t => t.AnyPoint(p => hiSegs.Any(ls => ls.ContainsVertex(p)))).ToArray();

        // for (var i = 0; i < hiSegs.Count; i++)
        // {
        //     var loIndex = loSegs.Count - 1 - i;
        //     var loP = loSegs[loIndex].To;
        //     var hiP = hiSegs[i].From;
        //     var loTris = loEdgeTris.Where(t => t.PointIsVertex(loP)).ToArray();
        //     var hiTris = hiEdgeTris.Where(t => t.PointIsVertex(hiP)).ToArray();
        //     foreach (var loTri in loTris)
        //     {
        //         var loPos = new PolyTriPosition(lo.Id, loTri.Index);
        //         foreach (var hiTri in hiTris)
        //         {
        //             var hiPos = new PolyTriPosition(hi.Id, hiTri.Index);
        //             // foreignNeighbors.Add(new Edge<PolyTriPosition>(loPos, hiPos, ptp => ptp.PolyId));
        //         }
        //     }
        // }
        //
        // void linkP()
        // {
        //     
        // }
    }

    private void Postprocess(GenWriteKey key)
    {
        var polys = key.Data.Planet.Polygons.Entities;
        var erodeChance = .75f;
        var mountainNoise = new FastNoiseLite();
        mountainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        mountainNoise.Frequency = .002f;
        var swampNoise = new FastNoiseLite();
        swampNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        swampNoise.Frequency = .01f;
        Parallel.ForEach(polys, poly =>
        {
            foreach (var tri in poly.Tris.Tris)
            {
                erode(poly, tri);
                irrigate(poly, tri);
                mountainRidging(poly, tri);
                swampRidging(poly, tri);
            }
        });

        void erode(MapPolygon poly, PolyTri tri)
        {
            if (
                (tri.Landform == LandformManager.Mountain || tri.Landform == LandformManager.Peak)
                && tri.AnyNeighbor(poly, n => n.Landform.IsWater)
                && Game.I.Random.Randf() < erodeChance
            )
            {
                var v = key.Data.Models.Vegetation.GetAtPoint(poly, tri.GetCentroid(), 
                    LandformManager.Hill, _data);
                tri.SetLandform(LandformManager.Hill, key);
                tri.SetVegetation(v, key);
            }
        }

        void irrigate(MapPolygon poly, PolyTri tri)
        {
            if (tri.Landform.IsLand
                && tri.Vegetation.MinMoisture < VegetationManager.Grassland.MinMoisture
                && VegetationManager.Grassland.AllowedLandforms.Contains(tri.Landform)
                && tri.AnyNeighbor(poly, n => n.Landform.IsWater))
            {
                tri.SetVegetation(VegetationManager.Grassland, key);
                tri.ForEachNeighbor(poly, nTri =>
                {
                    if (nTri.Landform.IsLand
                        && nTri.Vegetation.MinMoisture < VegetationManager.Steppe.MinMoisture
                        && VegetationManager.Steppe.AllowedLandforms.Contains(nTri.Landform))
                    {
                        nTri.SetVegetation(VegetationManager.Steppe, key);
                    }
                });
            }
        }

        void swampRidging(MapPolygon poly, PolyTri tri)
        {
            if (tri.Vegetation == VegetationManager.Swamp)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = swampNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if (noise < -.3f)
                {
                    tri.SetVegetation(VegetationManager.Forest, key);
                }
                else if (noise < -.2f)
                {
                    tri.SetVegetation(VegetationManager.Grassland, key);
                }
            }
        }

        void mountainRidging(MapPolygon poly, PolyTri tri)
        {
            if (tri.Landform.IsLand && tri.Landform.MinRoughness >= LandformManager.Peak.MinRoughness)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = mountainNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if(noise < .2f) tri.SetLandform(LandformManager.Mountain, key);
            }
            else if (tri.Landform.IsLand && tri.Landform.MinRoughness >= LandformManager.Mountain.MinRoughness)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = mountainNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if(noise < 0f)
                {
                    tri.SetLandform(LandformManager.Hill, key);
                    tri.SetVegetation(_data.Models.Vegetation.GetAtPoint(poly, tri.GetCentroid(), LandformManager.Hill, _data), key);
                }
            }
        }
    }
}
