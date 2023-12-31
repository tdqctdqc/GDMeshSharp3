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
    protected Node2D _graphicLayersParent;
    public MapOverlayDrawer Highlighter { get; private set; }
    public MapOverlayDrawer DebugOverlay { get; private set; }
    public GraphicLayerHolder GraphicLayerHolder { get; private set; }
    public ConcurrentQueue<Action> UpdateQueue { get; private set; }
    private int _msToProcessUpdates = 50;
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }

    public MapGraphics(Client client)
    {
        var sw = new Stopwatch();
        sw.Start();

        UpdateQueue = new ConcurrentQueue<Action>();
        
        _segmenter = new GraphicsSegmenter(10, client.Data);
        AddChild(_segmenter);
        _graphicLayersParent = new Node2D();
        AddChild(_graphicLayersParent);
        GraphicLayerHolder = new GraphicLayerHolder(client, _segmenter, _graphicLayersParent, client.Data);
        DebugOverlay = new MapOverlayDrawer(_segmenter, 98);
        Highlighter = new MapOverlayDrawer(_segmenter, 99);
        
        client.GraphicsLayer.AddChild(this);
        
        sw.Stop();
        client.Data.Logger.Log("map graphics setup time " + sw.Elapsed.TotalMilliseconds, LogType.Graphics);
    }
    private MapGraphics()
    {
        
    }
    public void Process(float delta)
    {
        while (UpdateQueue.TryDequeue(out var u))
        {
            u?.Invoke();
        }
        if(Game.I.Client?.Cam() is ICameraController c)
        {
            _segmenter.Update(c.XScrollRatio);
        }
    }
}