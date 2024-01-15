using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class PolyTriGenerator : Generator
{
    private GenData _data;
    public PolyTriGenerator()
    {
    }
    public override GenReport Generate(GenWriteKey key)
    {
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        var polys = _data.GetAll<MapPolygon>();
        var polyEdges = _data.GetAll<MapPolygonEdge>();
        
        report.StartSection();
        var riverData = new RiverPolyTriGen().DoRivers(key);
        report.StopSection("Finding rivers");
        
        report.StartSection();

        var cells = polys.AsParallel()
            .Select(p =>
            {
                var cells = BuildTris(p, riverData, key);
                return (p, cells);
            })
            .ToArray()
            .SelectMany(v => v.cells)
            .ToArray();
        PolyCells.Create(cells, key);
        report.StopSection("Building poly terrain tris");
        
        report.StartSection();
        var edgeTris = new ConcurrentBag<(PolyTriPosition[], PolyTriPosition[])>();
        report.StopSection("making poly tri paths");
        
        _data.Notices.SetPolyShapes.Invoke();
        
        report.StartSection();
        Postprocess(key);
        report.StopSection("postprocessing polytris");

        report.StartSection();
        Parallel.ForEach(polys, p => p.SetTerrainStats(key)); 
        report.StopSection("setting terrain stats");

        return report;
    }
    private PolyCell[] BuildTris(MapPolygon poly, TempRiverData rd, GenWriteKey key)
    {
        (List<PolyTri> tris, PolyCell[] cells) divs;
        if (poly.IsWater())
        {
            divs = DoSeaPoly(poly, key);
        }
        else if (poly.GetNexi(key.Data).Any(n => n.IsRiverNexus(key.Data)))
        {
            divs = RiverTriGen.DoPoly(poly, key.Data, rd, key);
        }
        else
        {
            divs = DoLandPolyNoRivers(poly, key);
        }

        var polyTerrainTris = PolyTris.Create(divs.tris,  key);
        if (polyTerrainTris == null) throw new Exception();
        poly.SetTerrainTris(polyTerrainTris, key);
        return divs.cells;
    }
    private (List<PolyTri>, PolyCell[]) DoSeaPoly(MapPolygon poly, GenWriteKey key)
    {
        var tris = new List<PolyTri>();
        var boundaryPs = poly.GetOrderedBoundaryPoints(_data);
        var triPIndices = Geometry2D.TriangulatePolygon(boundaryPs);
        for (var i = 0; i < triPIndices.Length; i+=3)
        {
            var a = boundaryPs[triPIndices[i]];
            var b = boundaryPs[triPIndices[i+1]];
            var c = boundaryPs[triPIndices[i+2]];
            tris.Add(PolyTri.Construct(poly.Id, a,b,c,_data.Models.Landforms.Sea,
                _data.Models.Vegetations.Barren));
        }

        var cell = new PolyCell(
            poly.Center, boundaryPs.ToArray(),
            key.Data.Models.Vegetations.Barren.MakeRef(),
            key.Data.Models.Landforms.Sea.MakeRef()
        );
        return (tris, new PolyCell[]{cell});
    }
    
    private (List<PolyTri>, PolyCell[]) DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderPs = poly.GetOrderedBoundaryPoints(_data);
        List<PolyTri> tris = borderPs.PolyTriangulate(key.Data, poly);
        
        
        
        return (tris, GetLandCellsForBoundary(borderPs, poly, key.Data));
    }

    public static PolyCell[] GetLandCellsForBoundary(Vector2[] boundary,
        MapPolygon poly, Data d)
    {
        var boundaries = GraphGenerator
            .GenerateVoronoiPolysForInterior(boundary);
        var cells = boundaries.Select(b =>
        {
            var lf = d.Models.Landforms.GetAtPoint(poly, b.First(), d);
            var v = d.Models.Vegetations.GetAtPoint(poly, b.First(), lf, d);
            var cell = new PolyCell(poly.Center, b,
                v.MakeRef(), lf.MakeRef());
            return cell;
        }).ToArray();
        return cells;
    }
    private void Postprocess(GenWriteKey key)
    {
        var polys = key.Data.GetAll<MapPolygon>();
        var erodeChance = .75f;
        var mountainNoise = new FastNoiseLite();
        mountainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        mountainNoise.Frequency = .002f;
        var swampNoise = new FastNoiseLite();
        swampNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        swampNoise.Frequency = .005f;
        var swampWideNoise = new FastNoiseLite();
        swampWideNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        swampWideNoise.Frequency = 300f;
        
        var grassland = key.Data.Models.Vegetations.Grassland;
        var tundra = key.Data.Models.Vegetations.Tundra;

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
                (tri.Landform(_data) == _data.Models.Landforms.Mountain || tri.Landform(_data) == _data.Models.Landforms.Peak)
                && tri.AnyNeighbor(poly, n => n.Landform(_data).IsWater)
                && Game.I.Random.Randf() < erodeChance
            )
            {
                var v = key.Data.Models.Vegetations.GetAtPoint(poly, tri.GetCentroid(), 
                    _data.Models.Landforms.Hill, _data);
                tri.SetLandform(_data.Models.Landforms.Hill, key);
                tri.SetVegetation(v, key);
            }
        }

        void irrigate(MapPolygon poly, PolyTri tri)
        {
            if (poly.DistFromEquatorRatio(_data) >= tundra.MinDistFromEquatorRatio) return;
            if (tri.Landform(_data).IsLand
                && tri.Vegetation(_data).MinMoisture < _data.Models.Vegetations.Grassland.MinMoisture
                && _data.Models.Vegetations.Grassland.AllowedLandforms.Contains(tri.Landform(_data))
                && tri.AnyNeighbor(poly, n => n.Landform(_data).IsWater))
            {
                tri.SetVegetation(_data.Models.Vegetations.Grassland, key);
                tri.ForEachNeighbor(poly, nTri =>
                {
                    if (nTri.Landform(_data).IsLand
                        && nTri.Vegetation(_data).MinMoisture < _data.Models.Vegetations.Steppe.MinMoisture
                        && _data.Models.Vegetations.Steppe.AllowedLandforms.Contains(nTri.Landform(_data)))
                    {
                        nTri.SetVegetation(_data.Models.Vegetations.Steppe, key);
                    }
                });
            }
        }

        void swampRidging(MapPolygon poly, PolyTri tri)
        {
            if (tri.Vegetation(_data) == _data.Models.Vegetations.Swamp)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = swampNoise.GetNoise2D(globalPos.X, globalPos.Y);
                var wideNoise = swampWideNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if (noise < -.3f && wideNoise < 0f)
                {
                    tri.SetVegetation(_data.Models.Vegetations.Forest, key);
                }
                else if (noise < -.2f && wideNoise < 0f)
                {
                    
                    tri.SetVegetation(_data.Models.Vegetations.Grassland, key);
                }
            }
        }

        void mountainRidging(MapPolygon poly, PolyTri tri)
        {
            if (tri.Landform(_data).IsLand && tri.Landform(_data).MinRoughness >= _data.Models.Landforms.Peak.MinRoughness)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = mountainNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if(noise < .2f) tri.SetLandform(_data.Models.Landforms.Mountain, key);
            }
            else if (tri.Landform(_data).IsLand && tri.Landform(_data).MinRoughness >= _data.Models.Landforms.Mountain.MinRoughness)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = mountainNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if(noise < 0f)
                {
                    tri.SetLandform(_data.Models.Landforms.Hill, key);
                    tri.SetVegetation(_data.Models.Vegetations.GetAtPoint(poly, tri.GetCentroid(), _data.Models.Landforms.Hill, _data), key);
                }
            }
        }
    }
}
