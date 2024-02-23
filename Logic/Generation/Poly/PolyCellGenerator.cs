using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class PolyCellGenerator : Generator
{
    private GenData _data;
    public PolyCellGenerator()
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

        var cellsByPoly = polys.AsParallel()
            .Select(p =>
            {
                var cells = BuildCells(p, riverData, key);
                return (p, cells);
            })
            .ToDictionary(v => v.p, v => v.cells);
        var cells = cellsByPoly.SelectMany(kvp => kvp.Value).ToArray();
        
        report.StopSection("Building poly terrain tris and cells");
        
        PolyCells.Create(cells, key);
        
        _data.Notices.SetPolyShapes.Invoke();

        report.StartSection();
        Postprocess(key);
        report.StopSection("postprocessing polytris");
        
        _data.Notices.MadeCells.Invoke();


        report.StartSection();
        Parallel.ForEach(polys, p => p.SetTerrainStats(key)); 
        report.StopSection("setting terrain stats");
        return report;
    }
    private PolyCell[] BuildCells(MapPolygon poly, TempRiverData rd, GenWriteKey key)
    {
        PolyCell[] cells;
        var preCells = key.GenData.GenAuxData.PreCellPolys[poly];
        if (poly.IsWater())
        {
            cells = preCells.Select(p =>
                SeaCell.Construct(p, key)).ToArray();
        }
        // else if (poly.GetNexi(key.Data).Any(n => n.IsRiverNexus(key.Data)))
        // {
        //     cells = RiverTriGen.DoPoly(poly, key.Data, rd, key);
        // }
        else
        {
            cells = preCells
                .Select(p => LandCell.Construct(p, key))
                .ToArray();
        }
        
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


        var landCells = key.Data.Planet.PolygonAux.PolyCells.Cells.Values
            .OfType<LandCell>();
        var landCellsByPoly = landCells
            .SortInto(l => l.Polygon.Entity(key.Data));
        Parallel.ForEach(landCellsByPoly, kvp =>
        {
            var cells = kvp.Value;
            var poly = kvp.Key;
            foreach (var cell in cells)
            {
                erode(poly, cell);
                irrigate(poly, cell);
                mountainRidging(poly, cell);
                swampRidging(cell);
            }
        });

        void erode(MapPolygon poly, PolyCell cell)
        {
            if (
                (cell.GetLandform(_data) == _data.Models.Landforms.Mountain || cell.GetLandform(_data) == _data.Models.Landforms.Peak)
                && cell.AnyNeighbor(n => n.GetLandform(_data).IsWater, _data)
                && Game.I.Random.Randf() < erodeChance
            )
            {
                var v = key.Data.Models.Vegetations.GetAtPoint(poly, 
                    poly.GetOffsetTo(cell.GetCenter(), key.Data), 
                    _data.Models.Landforms.Hill, _data);
                cell.SetLandform(_data.Models.Landforms.Hill, key);
                cell.SetVegetation(v, key);
            }
        }

        void irrigate(MapPolygon poly, PolyCell cell)
        {
            if (poly.DistFromEquatorRatio(_data) >= tundra.MinDistFromEquatorRatio) return;
            if (cell.GetLandform(_data).IsLand
                && cell.GetVegetation(_data).MinMoisture < _data.Models.Vegetations.Grassland.MinMoisture
                && _data.Models.Vegetations.Grassland.AllowedLandforms.Contains(cell.GetLandform(_data))
                && cell.AnyNeighbor(n => n.GetLandform(_data).IsWater, _data))
            {
                cell.SetVegetation(_data.Models.Vegetations.Grassland, key);
                cell.ForEachNeighbor(nCell =>
                {
                    if (nCell.GetLandform(_data).IsLand
                        && nCell.GetVegetation(_data).MinMoisture < _data.Models.Vegetations.Steppe.MinMoisture
                        && _data.Models.Vegetations.Steppe.AllowedLandforms.Contains(nCell.GetLandform(_data)))
                    {
                        nCell.SetVegetation(_data.Models.Vegetations.Steppe, key);
                    }
                }, _data);
            }
        }

        void swampRidging(PolyCell cell)
        {
            if (cell.Vegetation.Model(_data) == _data.Models.Vegetations.Swamp)
            {
                var globalPos = cell.GetCenter();
                var noise = swampNoise.GetNoise2D(globalPos.X, globalPos.Y);
                var wideNoise = swampWideNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if (noise < -.3f && wideNoise < 0f)
                {
                    cell.SetVegetation(_data.Models.Vegetations.Forest, key);
                }
                else if (noise < -.2f && wideNoise < 0f)
                {
                    
                    cell.SetVegetation(_data.Models.Vegetations.Grassland, key);
                }
            }
        }

        void mountainRidging(MapPolygon poly, PolyCell cell)
        {
            if (cell.GetLandform(_data).IsLand && cell.GetLandform(_data).MinRoughness >= _data.Models.Landforms.Peak.MinRoughness)
            {
                var globalPos = cell.GetCenter();
                var noise = mountainNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if(noise < .2f) cell.SetLandform(_data.Models.Landforms.Mountain, key);
            }
            else if (cell.GetLandform(_data).IsLand && cell.GetLandform(_data).MinRoughness >= _data.Models.Landforms.Mountain.MinRoughness)
            {
                var globalPos = cell.GetCenter();
                var noise = mountainNoise.GetNoise2D(globalPos.X, globalPos.Y);
                if(noise < 0f)
                {
                    cell.SetLandform(_data.Models.Landforms.Hill, key);
                    cell.SetVegetation(_data.Models.Vegetations.GetAtPoint(poly, poly.Center.Offset(cell.GetCenter(), key.Data), _data.Models.Landforms.Hill, _data), key);
                }
            }
        }
    }
}
