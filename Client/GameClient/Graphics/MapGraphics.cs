using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class MapGraphics : Node2D, IClientComponent
{
    
    protected GraphicsSegmenter _segmenter;
    public PolyHighlighter Highlighter { get; private set; }
    public GraphicLayerHolder GraphicLayerHolder { get; private set; }
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }

    public MapGraphics(Data data, Client client)
    {
        GD.Print("making map graphics");
        Clear();
        var sw = new Stopwatch();
        sw.Start();
        
        _segmenter = new GraphicsSegmenter(10, data);
        AddChild(_segmenter);
        GraphicLayerHolder = new GraphicLayerHolder(_segmenter, this, data);

        Highlighter = new PolyHighlighter(data);
        AddChild(Highlighter);
        
        var inputCatcher = new MapInputCatcher(data);
        AddChild(inputCatcher);
        
        client.GraphicsLayer.AddChild(this);
        
        data.Notices.Ticked.Blank.SubscribeForNode(() => Update(data), this);

        sw.Stop();
        GD.Print("map graphics setup time " + sw.Elapsed.TotalMilliseconds);
    }
    private MapGraphics()
    {
        
    }
    private void Update(Data d)
    {
        GraphicLayerHolder.Update(d);
    }
    public void Process(float delta)
    {
        if(Game.I.Client?.Cam() is ICameraController c)
        {
            _segmenter.Update(c.XScrollRatio);
        }
    }
    private void Clear()
    {
        this.ClearChildren();
    }
}