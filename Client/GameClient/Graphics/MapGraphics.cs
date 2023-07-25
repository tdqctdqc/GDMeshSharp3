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
    protected GraphicsSegmenter _segmenter;
    public PolyHighlighter Highlighter { get; private set; }
    public GraphicLayerHolder GraphicLayerHolder { get; private set; }
    private ClientWriteKey _key;
    public void Setup(ClientWriteKey key)
    {
        Clear();
        _key = key;
        var sw = new Stopwatch();
        sw.Start();
        
        _key.Session.Client.UiRequests.ToggleMapGraphicsLayer.SubscribeForNode(ToggleMapLayer, this);
        
        _segmenter = new GraphicsSegmenter(10, _key.Data);
        AddChild(_segmenter);
        GraphicLayerHolder = new GraphicLayerHolder(_segmenter, this, _key.Data);

        Highlighter = new PolyHighlighter(_key.Data);
        AddChild(Highlighter);
        
        var inputCatcher = new MapInputCatcher(_key, this);
        AddChild(inputCatcher);
        
        key.Data.Notices.Ticked.Blank.SubscribeForNode(() => Update(_key.Data), this);

        sw.Stop();
        GD.Print("map graphics setup time " + sw.Elapsed.TotalMilliseconds);
    }

    private void Update(Data d)
    {
        GraphicLayerHolder.Update(d);
    }
    private void ToggleMapLayer(string name)
    {
        // foreach (var mc in MapChunkGraphics)
        // {
        //     var n = mc.Modules[name];
        //     n.Hidden = n.Hidden == false;
        // }
    }
    public void Process(float delta)
    {
        // if (MapChunkGraphics != null)
        // {
        //     for (var i = 0; i < MapChunkGraphics.Count; i++)
        //     {
        //         MapChunkGraphics[i].UpdateVis();
        //     }
        // }
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