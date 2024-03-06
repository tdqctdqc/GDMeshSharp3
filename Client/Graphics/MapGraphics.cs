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
    public Regime SpectatingRegime { get; private set; }
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

        var localPlayerRegime = client.Data.BaseDomain.PlayerAux
            .LocalPlayer.Regime;
        if (localPlayerRegime.Fulfilled())
        {
            SpectateRegime(localPlayerRegime.Entity(client.Data));
        }
        else
        {
            SpectatingRegime = null;
        }
        
        client.Data.BaseDomain.PlayerAux
            .PlayerChangedRegime.SubscribeForNode(n =>
            {
                if (n.Entity == client.Data.BaseDomain.PlayerAux.LocalPlayer)
                {
                    SpectateRegime(n.Entity.Regime.Entity(client.Data));
                }
            }, this);

        UpdateQueue = new ConcurrentQueue<Action>();
        
        _segmenter = new GraphicsSegmenter(10, client.Data);
        AddChild(_segmenter);
        GraphicLayerHolder = new GraphicLayerHolder(client, _segmenter, client.Data);
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

    public void SpectateRegime(Regime r)
    {
        SpectatingRegime = r;
        Game.I.Client.Notices.ChangedSpectatingRegime.Invoke(r);
    }
}