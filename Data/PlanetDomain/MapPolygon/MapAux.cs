using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class MapAux
{
    public PolyGrid<MapPolygon> MapPolyGrid { get; private set; }
    public PolyGrid<Cell> CellGrid { get; private set; }
    public CellHolder CellHolder => _cells.Value;
    private SingletonCache<CellHolder> _cells;
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, MapChunk> ChunksByPoly { get; private set; }
    
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
        
        data.Notices.FinishedStateSync.Subscribe(() => BuildMapGrids(data));
        data.Notices.Gen.MadeCells.Subscribe(() => BuildMapGrids(data));
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

    private void BuildMapGrids(Data data)
    {
        BuildCells(data);
        BuildChunks(data);
    }
    private void BuildCells(Data data)
    {
        var sw = new Stopwatch();
        sw.Start();
        CellGrid = new PolyGrid<Cell>(
            data.Planet.Info.Dimensions, 
            500f,
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
                // CellsByPoly.GetOrAdd(e.Edge.Get(data).LowPoly.Get(data), p => new List<Cell>())
                //     .Add(element);
            }
        }
        sw.Stop();
        GD.Print("build cells time " + sw.Elapsed.TotalMilliseconds);
    }
    private void BuildChunks(Data data)
    {
        var sw = new Stopwatch();
        sw.Start();
        var polys = data.GetAll<MapPolygon>();
        ChunksByPoly = new Dictionary<MapPolygon, MapChunk>();
        Chunks = new HashSet<MapChunk>();
        var chunksByKey = new Dictionary<Vector2I, MapChunk>();
        foreach (var poly in polys)
        {
            var key = getChunkKey(poly);
            var cells = poly.GetCells(data);
            var chunk = chunksByKey.GetOrAdd(key, k =>
            {
                var chunk = new MapChunk(new List<MapPolygon>(),
                    new List<Cell>(), key, data);
                Chunks.Add(chunk);
                return chunk;
            });
            chunk.Polys.Add(poly);
            chunk.Cells.AddRange(cells);
            ChunksByPoly.Add(poly, chunk);
        }
        sw.Stop();
        GD.Print("build chunks time " + sw.Elapsed.TotalMilliseconds);

        
        sw.Reset();
        sw.Start();

        Parallel.ForEach(Chunks, mapChunk => mapChunk.SetVertexInfos(data));
        sw.Stop();
        GD.Print("build chunk vertex info time " + sw.Elapsed.TotalMilliseconds);

        Vector2I getChunkKey(MapPolygon poly)
        {
            return new Vector2I((int)(poly.Center.X / MapChunk.ChunkDim),
                (int)(poly.Center.Y / MapChunk.ChunkDim));
        }
        
    }
}