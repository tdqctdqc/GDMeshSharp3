using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolygonAux : EntityAux<MapPolygon>
{
    public IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph { get; private set; }
    public EntityValueCache<MapPolygon, PolyAuxData> AuxDatas { get; private set; }
    public PolyGrid MapPolyGrid { get; private set; }
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, MapChunk> ChunksByPoly { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public ValChangeAction<Regime> ChangedRegime { get; private set; }
    public MapPolygonAux(Data data) : base(data)
    {
        BorderGraph = ImplicitGraph.Get<MapPolygon, PolyBorderChain>(
            n => n.Neighbors.Entities(data),
            (n, m) => n.GetEdge(m, data).GetSegsRel(n, data),
            () => Register.Entities, 
            () => Register.Entities.SelectMany(e => e.GetPolyBorders()).ToHashSet()
        );
        AuxDatas = EntityValueCache<MapPolygon, PolyAuxData>.ConstructConstant(
            data,
            p => new PolyAuxData(p, data)
        );
        
        ChangedRegime = new ValChangeAction<Regime>();
        
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
        var sw = new Stopwatch();
        var gridCellSize = 1000f;
        var numPartitions = Mathf.CeilToInt(data.Planet.Info.Dimensions.X / gridCellSize);
        MapPolyGrid = new PolyGrid(numPartitions, data.Planet.Info.Dimensions, data);
        foreach (var p in data.Planet.Polygons.Entities)
        {
            if(p.NeighborBorders.Count > 0) MapPolyGrid.AddElement(p);
        }
        MapPolyGrid.Update();
    }
    private void BuildChunks(Data data)
    {
        var regularGrid = new RegularGrid<MapPolygon>
        (
            polygon => polygon.Center,
            data.Planet.Width / 10f
        );
        foreach (var p in data.Planet.Polygons.Entities)
        {
            regularGrid.AddElement(p);
        }
        regularGrid.Update();
        ChunksByPoly = new Dictionary<MapPolygon, MapChunk>();
        Chunks = new HashSet<MapChunk>();
        foreach (var c in regularGrid.Cells)
        {
            var chunk = new MapChunk(c.Value, c.Key);
            Chunks.Add(chunk);
            c.Value.ForEach(p => ChunksByPoly.Add(p, chunk));
        }
    }
}