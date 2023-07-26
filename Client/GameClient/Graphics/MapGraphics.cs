using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class MapGraphics : Node2D
{
    
    protected GraphicsSegmenter _segmenter;
    public PolyHighlighter Highlighter { get; private set; }
    public GraphicLayerHolder GraphicLayerHolder { get; private set; }
    private ClientWriteKey _key;

    public MapGraphics(ClientWriteKey key)
    {
        Clear();
        _key = key;
        var sw = new Stopwatch();
        sw.Start();
        
        _segmenter = new GraphicsSegmenter(10, _key.Data);
        AddChild(_segmenter);
        GraphicLayerHolder = new GraphicLayerHolder(_segmenter, this, _key.Data);

        Highlighter = new PolyHighlighter(_key.Data);
        AddChild(Highlighter);
        
        var inputCatcher = new MapInputCatcher(_key, this);
        AddChild(inputCatcher);
        
        GD.Print("enrolling mapGraphics");
        key.Data.Notices.Ticked.Blank.SubscribeForNode(() => Update(_key.Data), this);

        sw.Stop();
        GD.Print("map graphics setup time " + sw.Elapsed.TotalMilliseconds);
    }
    private MapGraphics()
    {
        
    }
    private void Update(Data d)
    {
        GD.Print("updating " + GetHashCode());
        GraphicLayerHolder.Update(d);
    }
    public void Process(float delta)
    {
        if(Game.I.Client?.Cam != null)
        {
            _segmenter.Update(Game.I.Client.Cam.XScrollRatio);
        }
    }
    private void Clear()
    {
        this.ClearChildren();
    }
}