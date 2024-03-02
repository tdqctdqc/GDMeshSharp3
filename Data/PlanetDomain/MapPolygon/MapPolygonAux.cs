using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolygonAux
{
    public PolyGrid<MapPolygon> MapPolyGrid { get; private set; }
    public PolyGrid<Cell> CellGrid { get; private set; }
    public PolyCells PolyCells => _polyCells.Value;
    private SingletonAux<PolyCells> _polyCells;
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, MapChunk> ChunksByPoly { get; private set; }
    public Dictionary<Cell, MapChunk> ChunksByCell { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public ValChangeAction<MapPolygon, Regime> ChangedOwnerRegime { get; private set; }
    public ValChangeAction<MapPolygon, Regime> ChangedOccupierRegime { get; private set; }
    public EntityMultiIndexer<Regime, MapPolygon> PolysByRegime { get; private set; }
    public Dictionary<MapPolygon, List<Cell>> CellsByPoly { get; private set; }
    
    public MapPolygonAux(Data data)
    {
        _polyCells = new SingletonAux<PolyCells>(data);
        
        ChangedOwnerRegime = new ValChangeAction<MapPolygon, Regime>();
        ChangedOccupierRegime = new ValChangeAction<MapPolygon, Regime>();

        PolysByRegime = new EntityMultiIndexer<Regime, MapPolygon>(data,
            p => p.OwnerRegime.Entity(data), 
            new RefAction[] { }, ChangedOwnerRegime);
        
        data.Notices.SetLandAndSea.Subscribe(() =>
        {
            LandSea = new LandSeaManager();
            LandSea.SetMasses(data);
        });
        data.Notices.FinishedStateSync.Subscribe(() =>
        {
            LandSea = new LandSeaManager();
            LandSea.SetMasses(data);
        });
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildPolyGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildPolyGrid(data));
        
        // data.Notices.SetPolyShapes.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedGen.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
        data.Notices.MadeCells.Subscribe(() => BuildChunks(data));
        
        data.Notices.MadeCells.Subscribe(() => BuildCells(data));
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
                 data.GetAll<PolyCells>().First().Cells.Values)
        {
            CellGrid.AddElement(element);
            
            if (element is IPolyCell l)
            {
                CellsByPoly.GetOrAdd(l.Polygon.Entity(data), p => new List<Cell>())
                    .Add(element);
            }
            else if (element is IEdgeCell e)
            {
                CellsByPoly.GetOrAdd(e.Edge.Entity(data).HighPoly.Entity(data), p => new List<Cell>())
                    .Add(element);
                CellsByPoly.GetOrAdd(e.Edge.Entity(data).LowPoly.Entity(data), p => new List<Cell>())
                    .Add(element);
            }
        }
    }
    private void BuildChunks(Data data)
    {
        var polyGrid = new RegularGrid<MapPolygon>
        (
            polygon => polygon.Center,
            data.Planet.Width / 10f
        );
        var cellGrid = new RegularGrid<Cell>
        (
            c => c.GetCenter().ClampPosition(data),
            data.Planet.Width / 10f
        );
            
        foreach (var p in data.GetAll<MapPolygon>())
        {
            polyGrid.AddElement(p);
        }
        polyGrid.Update();
        
        foreach (var c in data.Planet.PolygonAux
                     .PolyCells.Cells.Values)
        {
            cellGrid.AddElement(c);
        }
        cellGrid.Update();
        
        
        ChunksByPoly = new Dictionary<MapPolygon, MapChunk>();
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
            polys.ForEach(p => ChunksByPoly.Add(p, chunk));
            cells.ForEach(c => ChunksByCell.Add(c, chunk));
        }
    }
}