using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolygonAux
{
    public IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph { get; private set; }
    public EntityValueCache<MapPolygon, PolyAuxData> AuxDatas { get; private set; }
    public PolyGrid<MapPolygon> MapPolyGrid { get; private set; }
    public PolyGrid<PolyCell> PolyCellGrid { get; private set; }
    public PolyCells PolyCells => _polyCells.Value;
    private SingletonAux<PolyCells> _polyCells;
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, MapChunk> ChunksByPoly { get; private set; }
    public Dictionary<PolyCell, MapChunk> ChunksByCell { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public ValChangeAction<MapPolygon, Regime> ChangedOwnerRegime { get; private set; }
    public ValChangeAction<MapPolygon, Regime> ChangedOccupierRegime { get; private set; }
    public EntityMultiIndexer<Regime, MapPolygon> PolysByRegime { get; private set; }
    public Dictionary<MapPolygon, List<PolyCell>> CellsByPoly { get; private set; }
    
    public MapPolygonAux(Data data)
    {
        _polyCells = new SingletonAux<PolyCells>(data);
        BorderGraph = ImplicitGraph.Get<MapPolygon, PolyBorderChain>(
            n => n.Neighbors.Items(data),
            (n, m) => n.GetEdge(m, data).GetSegsRel(n, data),
            () => data.GetAll<MapPolygon>(), 
            () => data.GetAll<MapPolygon>().SelectMany(e => e.GetPolyBorders()).ToHashSet()
        );
        AuxDatas = EntityValueCache<MapPolygon, PolyAuxData>.ConstructConstant(
            data,
            p => new PolyAuxData(p, data)
        );
        
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
        
        data.Notices.SetPolyShapes.Subscribe(() => UpdateAuxDatas(data));
        data.Notices.FinishedStateSync.Subscribe(() => UpdateAuxDatas(data));
        
        data.Notices.MadeCells.Subscribe(() => BuildCells(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildCells(data));
    }

    private void UpdateAuxDatas(Data data)
    {
        foreach (var kvp in AuxDatas.Dic)
        {
            var aux = kvp.Value;
            if (aux.Stale)
            {
                var poly = kvp.Key;
                aux.Update(poly, data);
                aux.MarkFresh();
            }
        }
    }
    private void BuildPolyGrid(Data data)
    {
        MapPolyGrid = new PolyGrid<MapPolygon>(
            data.Planet.Info.Dimensions, 
            300f,
            p => p.GetOrderedBoundaryPoints(data),
            p => p.Center);
        foreach (var element in data.GetAll<MapPolygon>())
        {
            MapPolyGrid.AddElement(element);
        }
    }

    private void BuildCells(Data data)
    {
        PolyCellGrid = new PolyGrid<PolyCell>(
            data.Planet.Info.Dimensions, 
            100f,
            p => p.RelBoundary,
            p => p.RelTo);
        CellsByPoly = new Dictionary<MapPolygon, List<PolyCell>>();
        foreach (var element in 
                 data.GetAll<PolyCells>().First().Cells.Values)
        {
            PolyCellGrid.AddElement(element);
            
            if (element is ISinglePolyCell l)
            {
                CellsByPoly.GetOrAdd(l.Polygon.Entity(data), p => new List<PolyCell>())
                    .Add(element);
            }
            else if (element is IEdgeCell e)
            {
                CellsByPoly.GetOrAdd(e.Edge.Entity(data).HighPoly.Entity(data), p => new List<PolyCell>())
                    .Add(element);
                CellsByPoly.GetOrAdd(e.Edge.Entity(data).LowPoly.Entity(data), p => new List<PolyCell>())
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
        var cellGrid = new RegularGrid<PolyCell>
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
        ChunksByCell = new Dictionary<PolyCell, MapChunk>();
        Chunks = new HashSet<MapChunk>();
        var keys = cellGrid.Cells.Keys
            .Union(polyGrid.Cells.Keys)
            .ToHashSet();
        
        foreach (var key in keys)
        {
            var cells = cellGrid.Cells.ContainsKey(key)
                ? cellGrid.Cells[key]
                : new List<PolyCell>();
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