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
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, MapChunk> ChunksByPoly { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public ValChangeAction<MapPolygon, Regime> ChangedOwnerRegime { get; private set; }
    public ValChangeAction<MapPolygon, Regime> ChangedOccupierRegime { get; private set; }
    public EntityMultiIndexer<Regime, MapPolygon> PolysByRegime { get; private set; }
    public MapPolygonAux(Data data)
    {
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
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
        
        data.Notices.SetPolyShapes.Subscribe(() => UpdateAuxDatas(data));
        data.Notices.FinishedStateSync.Subscribe(() => UpdateAuxDatas(data));
        
        data.Notices.FinishedGen.Subscribe(() => BuildCellGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildCellGrid(data));
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

    private void BuildCellGrid(Data data)
    {
        PolyCellGrid = new PolyGrid<PolyCell>(
            data.Planet.Info.Dimensions, 
            100f,
            p => p.RelBoundary,
            p => p.RelTo);
        foreach (var element in 
                 data.GetAll<PolyCells>().First().Cells.Values)
        {
            PolyCellGrid.AddElement(element);
        }
    }
    private void BuildChunks(Data data)
    {
        var regularGrid = new RegularGrid<MapPolygon>
        (
            polygon => polygon.Center,
            data.Planet.Width / 10f
        );
        foreach (var p in data.GetAll<MapPolygon>())
        {
            regularGrid.AddElement(p);
        }
        regularGrid.Update();
        ChunksByPoly = new Dictionary<MapPolygon, MapChunk>();
        Chunks = new HashSet<MapChunk>();
        foreach (var c in regularGrid.Cells)
        {
            var chunk = new MapChunk(c.Value, c.Key, data);
            Chunks.Add(chunk);
            c.Value.ForEach(p => ChunksByPoly.Add(p, chunk));
        }
    }
}