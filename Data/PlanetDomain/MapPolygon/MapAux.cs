using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapAux
{
    public PolyGrid<MapPolygon> MapPolyGrid { get; private set; }
    public PolyGrid<Cell> CellGrid { get; private set; }
    public CellHolder CellHolder => _cells.Value;
    private SingletonCache<CellHolder> _cells;
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<Cell, MapChunk> ChunksByCell { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public Dictionary<MapPolygon, List<Cell>> CellsByPoly { get; private set; }
    
    public MapAux(Data data)
    {
        _cells = new SingletonCache<CellHolder>(data);
        
        data.Notices.Gen.SetLandAndSea.Subscribe(() =>
        {
            LandSea = new LandSeaManager();
            LandSea.SetMasses(data);
        });
        data.Notices.FinishedStateSync.Subscribe(() =>
        {
            LandSea = new LandSeaManager();
            LandSea.SetMasses(data);
        });
        
        data.Notices.Gen.SetPolyShapes.Subscribe(() => BuildPolyGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildPolyGrid(data));
        
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
        data.Notices.Gen.MadeCells.Subscribe(() => BuildChunks(data));
        
        data.Notices.Gen.MadeCells.Subscribe(() => BuildCells(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildCells(data));
    }

    private void BuildPolyGrid(Data data)
    {
        MapPolyGrid = new PolyGrid<MapPolygon>(
            data.Planet.Info.Dimensions, 
            300f,
            p => p.BoundaryPoints,
            p => p.Center);
        foreach (var element in data.GetAll<MapPolygon>())
        {
            MapPolyGrid.AddElement(element);
        }
    }

    private void BuildCells(Data data)
    {
        CellGrid = new PolyGrid<Cell>(
            data.Planet.Info.Dimensions, 
            100f,
            p => p.RelBoundary,
            p => p.RelTo);
        CellsByPoly = new Dictionary<MapPolygon, List<Cell>>();
        foreach (var element in 
                 data.GetAll<CellHolder>().First().Cells.Values)
        {
            CellGrid.AddElement(element);
            
            if (element is IPolyCell l)
            {
                CellsByPoly.GetOrAdd(l.Polygon.Get(data), p => new List<Cell>())
                    .Add(element);
            }
            else if (element is IEdgeCell e)
            {
                CellsByPoly.GetOrAdd(e.Edge.Get(data).HighPoly.Get(data), p => new List<Cell>())
                    .Add(element);
                CellsByPoly.GetOrAdd(e.Edge.Get(data).LowPoly.Get(data), p => new List<Cell>())
                    .Add(element);
            }
        }
    }
    private void BuildChunks(Data data)
    {
        var polyGrid = new RegularGrid<MapPolygon>
        (
            polygon => polygon.Center,
            MapChunk.ChunkDim
        );
        var cellGrid = new RegularGrid<Cell>
        (
            c => c.GetCenter().ClampPosition(data),
            MapChunk.ChunkDim
        );
            
        foreach (var p in data.GetAll<MapPolygon>())
        {
            polyGrid.AddElement(p);
        }
        polyGrid.Update();
        
        foreach (var c in data.Planet.MapAux
                     .CellHolder.Cells.Values)
        {
            cellGrid.AddElement(c);
        }
        cellGrid.Update();
        
        
        ChunksByCell = new Dictionary<Cell, MapChunk>();
        Chunks = new HashSet<MapChunk>();
        var keys = cellGrid.Cells.Keys
            .Union(polyGrid.Cells.Keys)
            .ToHashSet();
        
        foreach (var key in keys)
        {
            var cells = cellGrid.Cells.ContainsKey(key)
                ? cellGrid.Cells[key]
                : new List<Cell>();
            var polys = polyGrid.Cells.ContainsKey(key)
                ? polyGrid.Cells[key]
                : new List<MapPolygon>();
            var chunk = new MapChunk(polys, cells, key, data);
            Chunks.Add(chunk);
            cells.ForEach(c => ChunksByCell.Add(c, chunk));
        }
    }
}