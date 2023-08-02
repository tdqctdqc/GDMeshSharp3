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
    public MapInputCatcher InputCatcher { get; private set; }
    public ConcurrentQueue<Action> UpdateQueue { get; private set; }
    private int _msToProcessUpdates = 50;
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }

    public MapGraphics(Data data, Client client)
    {
        var sw = new Stopwatch();
        sw.Start();

        UpdateQueue = new ConcurrentQueue<Action>();
        
        _segmenter = new GraphicsSegmenter(10, data);
        AddChild(_segmenter);
        GraphicLayerHolder = new GraphicLayerHolder(_segmenter, data);

        Highlighter = new PolyHighlighter(data);
        AddChild(Highlighter);
        
        InputCatcher = new MapInputCatcher(data);
        AddChild(InputCatcher);
        
        client.GraphicsLayer.AddChild(this);
        
        data.Notices.Ticked.Blank.SubscribeForNode(() => Task.Run(() => Update(data)), this);

        sw.Stop();
        Game.I.Logger.Log("map graphics setup time " + sw.Elapsed.TotalMilliseconds, LogType.Graphics);
    }
    private MapGraphics()
    {
        
    }
    private void Update(Data d)
    {
        GraphicLayerHolder.Update(d, UpdateQueue);
    }
    public void Process(float delta)
    {
        // var sw = new Stopwatch();
        // sw.Start();
        // while (UpdateQueue.Count() > 0 && sw.Elapsed.TotalMilliseconds < _msToProcessUpdates)
        // {
        //     UpdateQueue.TryDequeue(out var u);
        //     u?.Invoke();
        // }
        Task.Run(() =>
        {
            while (UpdateQueue.Count() > 0)
            {
                UpdateQueue.TryDequeue(out var u);
                u?.Invoke();
            }
        });
        if(Game.I.Client?.Cam() is ICameraController c)
        {
            _segmenter.Update(c.XScrollRatio);
        }
    }
}