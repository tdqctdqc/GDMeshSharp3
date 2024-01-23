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

        var cellsByPoly = polys.AsParallel()
            .Select(p =>
            {
                var cells = BuildTris(p, riverData, key);
                return (p, cells);
            })
            .ToDictionary(v => v.p, v => v.cells);
        var cells = cellsByPoly.SelectMany(kvp => kvp.Value).ToArray();
        
        report.StopSection("Building poly terrain tris and cells");
        
        PolyCells.Create(cells, key);
        
        
        
        
        report.StartSection();
        ConnectDiffPolyCells(cellsByPoly, key);
        report.StopSection("connecting cells");

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
    private PolyCell[] BuildTris(MapPolygon poly, TempRiverData rd, GenWriteKey key)
    {
        PolyCell[] cells;
        if (poly.IsWater())
        {
            cells = DoSeaPoly(poly, key);
        }
        // else if (poly.GetNexi(key.Data).Any(n => n.IsRiverNexus(key.Data)))
        // {
        //     cells = RiverTriGen.DoPoly(poly, key.Data, rd, key);
        // }
        else
        {
            cells = DoLandPolyNoRivers(poly, key);
        }
        
        return cells;
    }
    private PolyCell[] DoSeaPoly(MapPolygon poly, GenWriteKey key)
    {
        var boundaryPs = poly.GetOrderedBoundaryPoints(_data);
        var cell = SeaCell.Construct(poly, boundaryPs.ToArray(), key);
        return new PolyCell[]{cell};
    }
    
    private PolyCell[] DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderPs = poly.GetOrderedBoundaryPoints(_data);
        return GraphGenerator.GenerateAndConnectPolyCellsForInterior(poly, borderPs, key);
    }


    
    private void ConnectDiffPolyCells(
        Dictionary<MapPolygon, PolyCell[]> cellsByPoly, 
        GenWriteKey key)
    {
        var d = key.Data;
        var cellHolder = d.GetAll<PolyCells>().First();
        var cells = cellHolder.Cells;
        // MergeSmallRiverCellsInPolys(key, cellsByPoly);
        MergeRiverCellsAcrossEdges(key, cellHolder);
        ConnectAcrossNonRiverEdges(cellsByPoly, d, cells);
        ConnectLandCellsOverRiverEdges(key, cellHolder);
        // FixShortEdgeConnections(cellsByPoly, key);
    }

    private void ConnectAcrossNonRiverEdges(
        Dictionary<MapPolygon, PolyCell[]> cellsByPoly, 
        Data d, Dictionary<int, PolyCell> cells)
    {
        var links = new ConcurrentBag<Vector2I>();
        
        Parallel.ForEach(_data.GetAll<MapPolygonEdge>(),
            e =>
            {
                // if (e.IsRiver()) return;
                var hi = e.HighPoly.Entity(d);
                var lo = e.LowPoly.Entity(d);
                if (hi.IsWater() && lo.IsWater())
                {
                    links.Add(new Vector2I(
                        cellsByPoly[hi][0].Id,
                        cellsByPoly[lo][0].Id));
                    return;
                }
                
                if (e.IsCoast(d))
                {
                    var water = hi.IsWater() ? hi : lo;
                    var land = hi.IsWater() ? lo : hi;
                    PolyCell.ConnectCellsByEdge(
                        cellsByPoly[land],
                        cellsByPoly[water],
                        water.Center,
                        (v, w) => links.Add(new Vector2I(v.Id, w.Id)),
                        d);
                    return;
                }
                
                PolyCell.ConnectOverMapPolyEdge(e, cellsByPoly,
                    (v, w) => links.Add(new Vector2I(v.Id, w.Id)),
                    d);
            }
        );
        foreach (var vector2I in links)
        {
            var c1 = cells[vector2I.X];
            var c2 = cells[vector2I.Y];
            c1.Neighbors.Add(c2.Id);
            c2.Neighbors.Add(c1.Id);
        }
    }

    private static void MergeSmallRiverCellsInPolys(
        GenWriteKey key, Dictionary<MapPolygon, PolyCell[]> cellsByPoly)
    {
        Parallel.ForEach(cellsByPoly, kvp =>
        {
            
        });
    }
    
    private static void MergeRiverCellsAcrossEdges(GenWriteKey key, 
        PolyCells cellHolder)
    {
        var cells = cellHolder.Cells;
        var d = key.Data;
        var rCellsByEdge = cells
            .Values.OfType<RiverCell>()
            .SortInto(c => c.Edge.RefId);

        foreach (var kvp in rCellsByEdge)
        {
            var edge = d.Get<MapPolygonEdge>(kvp.Key);
            var thisRCells = kvp.Value;
            if (thisRCells.Count() != 2)
            {
                // GD.Print("multi cell river cell edge");
                continue;
            }
        
            var first = thisRCells[0];
            var second = thisRCells[1];
            var offset = first.RelTo.GetOffsetTo(second.RelTo, d);
        
            var secondBoundaryTransposed = second.CoordinateBoundary(first, d);
            var union = Geometry2D.MergePolygons(
                first.RelBoundary,
                secondBoundaryTransposed);
            if (union.Count() != 1)
            {
                continue;
            }
        
            var newRiverCell = RiverCell.Construct(
                edge, first.RelTo,
                union.First(), key);
            newRiverCell.Neighbors.AddRange(first.Neighbors);
            newRiverCell.Neighbors.AddRange(second.Neighbors);
            foreach (var n1 in first.Neighbors)
            {
                if (cells.ContainsKey(n1) == false)
                {
                    throw new Exception($"{first.Id} neighbor {n1} doest not exist");
                }
                var cell = cells[n1];
                cell.Neighbors.Remove(first.Id);
                cell.Neighbors.Add(newRiverCell.Id);
            }
            first.Neighbors.Clear();
        
            foreach (var n2 in second.Neighbors)
            {
                if (cells.ContainsKey(n2) == false)
                {
                    throw new Exception($"{second.Id} neighbor {n2} doest not exist");
                }
                var cell = cells[n2];
                cell.Neighbors.Remove(second.Id);
                cell.Neighbors.Add(newRiverCell.Id);
            }
            second.Neighbors.Clear();
        
            cells.Remove(first.Id);
            cells.Remove(second.Id);
            cells.Add(newRiverCell.Id, newRiverCell);
        }

        
    }

    private void ConnectLandCellsOverRiverEdges(GenWriteKey key, 
        PolyCells cellHolder)
    {
        var rCells = cellHolder.Cells.Values.OfType<RiverCell>();
        var links = new ConcurrentBag<Vector2I>();
        Parallel.ForEach(rCells, rCell =>
        {
            var landCellNsByPoly = rCell.Neighbors.Select(n => cellHolder.Cells[n])
                .OfType<LandCell>().SortInto(c => c.Polygon.RefId);
            foreach (var kvp in landCellNsByPoly)
            {
                var landCells = kvp.Value;
                foreach (var landCell in landCells)
                {
                    var p = landCell.RelBoundary.First() + landCell.RelTo;
                    foreach (var kvp2 in landCellNsByPoly)
                    {
                        if (kvp2.Key == kvp.Key) continue;
                        var min = kvp2.Value.MinBy(c =>
                            (c.RelBoundary.First() + c.RelTo).GetOffsetTo(p, key.Data).Length());
                        links.Add(new Vector2I(landCell.Id, min.Id));
                    }
                }
            }
        });
        foreach (var vector2I in links)
        {
            var c1 = cellHolder.Cells[vector2I.X];
            var c2 = cellHolder.Cells[vector2I.Y];
            c1.Neighbors.Add(c2.Id);
            c2.Neighbors.Add(c1.Id);
        }
    }
    // private void FixShortEdgeConnections(Dictionary<MapPolygon, PolyCell[]> cellsByPoly, 
    //     GenWriteKey key)
    // {
    //     var cells = key.Data.GetAll<PolyCells>().First().Cells;
    //     var makeLinks = new ConcurrentBag<Vector2I>();
    //     var severLinks = new ConcurrentBag<Vector2I>();
    //     Parallel.ForEach(cells.Values.OfType<LandCell>(),
    //         landCell =>
    //         {
    //             doForSquares(landCell, (p1, p2, p3, p4) =>
    //             {
    //                 var length13 = p1.GetCenter().GetOffsetTo(p3.GetCenter(), key.Data).Length();
    //                 var length24 = p2.GetCenter().GetOffsetTo(p4.GetCenter(), key.Data).Length();
    //                 var (shortEdge, longEdge) = length13 < length24 
    //                     ? (new Vector2I(p1.Id, p3.Id), new Vector2I(p2.Id, p4.Id))
    //                     : (new Vector2I(p2.Id, p4.Id), new Vector2I(p1.Id, p3.Id));
    //                 if (p1.Neighbors.Contains(p3.Id) 
    //                     && p2.Neighbors.Contains(p4.Id))
    //                 {
    //                     severLinks.Add(longEdge);
    //                 }
    //                 else if (p1.Neighbors.Contains(p3.Id) == false
    //                          && p2.Neighbors.Contains(p4.Id) == false)
    //                 {
    //                     makeLinks.Add(shortEdge);
    //                 }
    //             });
    //         });
    //     
    //     // foreach (var (x, y) in severLinks)
    //     // {
    //     //     var c1 = cells[x];
    //     //     c1.Neighbors.Remove(y);
    //     //     var c2 = cells[y];
    //     //     c2.Neighbors.Remove(x);
    //     // }
    //     foreach (var (x, y) in makeLinks)
    //     {
    //         var c1 = cells[x];            
    //         c1.Neighbors.Add(y);
    //         var c2 = cells[y];
    //         c2.Neighbors.Add(x);
    //     }
    //     void doForSquares(PolyCell p1,
    //         Action<PolyCell, PolyCell, PolyCell, PolyCell> act)
    //     {
    //         foreach (var p2 in getNeighbors(p1.Id, p1))
    //         {
    //             foreach (var p3 in getNeighbors(p1.Id, p2, p1))
    //             {
    //                 foreach (var p4 in getNeighbors(p1.Id, p3, p1, p2)
    //                              .Where(p => p.Neighbors.Contains(p1.Id)))
    //                 {
    //                     act(p1, p2, p3, p4);
    //                 }
    //             }
    //         }
    //     }
    //
    //     IEnumerable<PolyCell> getNeighbors(int baseId, PolyCell p,
    //         PolyCell avoid1 = null, PolyCell avoid2 = null, PolyCell avoid3 = null)
    //     {
    //         return p.Neighbors.Select(n => cells[n])
    //             .Where(n => 
    //                 // n.Id > baseId 
    //             // && 
    //                 n is LandCell
    //             && n != avoid1
    //             && n != avoid2
    //             && n != avoid3);
    //     }
    // }
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
                    cell.SetVegetation(_data.Models.Vegetations.GetAtPoint(poly, poly.Center.GetOffsetTo(cell.GetCenter(), key.Data), _data.Models.Landforms.Hill, _data), key);
                }
            }
        }
    }
}
