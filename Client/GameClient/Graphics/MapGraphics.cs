using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class MapGraphics : Node2D
{
    public MapGraphics()
    {
        
    }
    protected List<IGraphicsSegmenter> _segmenters;
    public PolyHighlighter Highlighter { get; private set; }
    public List<MapChunkGraphic> MapChunkGraphics { get; private set; }
    public ChunkChangedCache ChunkChangedCache { get; private set; }
    private ClientWriteKey _key;
    public void Setup(ClientWriteKey key)
    {
        _key = key;
        var sw = new Stopwatch();
        sw.Start();
        ChunkChangedCache = new ChunkChangedCache(_key.Data);
        Clear();
        _segmenters = new List<IGraphicsSegmenter>();
        MapChunkGraphics = new List<MapChunkGraphic>();
        var polySegmenter = new GraphicsSegmenter<MapChunkGraphic>();
        _segmenters.Add(polySegmenter);
        
        var mapChunkGraphics = _key.Data.Planet.PolygonAux.Chunks.Select(u =>
        {
            var graphic = new MapChunkGraphic();
            MapChunkGraphics.Add(graphic);
            graphic.Setup(this, u, _key.Data);
            return graphic;
        }).ToList();
        
        
        foreach (var keyValuePair in MapChunkLayerBenchmark.Times)
        {
           GD.Print($"{keyValuePair.Key} {keyValuePair.Value.Sum()}"); 
        }
        
        polySegmenter.Setup(mapChunkGraphics, 10, n => n.Position, _key.Data);

        Highlighter = new PolyHighlighter(_key.Data);
        Highlighter.ZIndex = 99;
        AddChild(Highlighter);
        
        AddChild(polySegmenter);
        var inputCatcher = new MapInputCatcher(_key, this);
        AddChild(inputCatcher);
        
        sw.Stop();
        GD.Print("map graphics setup time " + sw.Elapsed.TotalMilliseconds);
    }

    public void Update()
    {
        MapChunkGraphics.ForEach(c =>
        {
            c.Update();
        });
        ChunkChangedCache.Clear();
    }


    public void Process(float delta)
    {
        if(Game.I.Client?.Cam != null)
        {
            if (_segmenters == null) return;
            for (var i = 0; i < _segmenters.Count; i++)
            {
                _segmenters[i].Update(Game.I.Client.Cam.XScrollRatio);
            }
        }
    }
    
    public override void _Input(InputEvent e)
    {
        
    }

    private void Clear()
    {
        _segmenters?.Clear();
        while (GetChildCount() > 0)
        {
            GetChild(0).Free();
        }
    }
}