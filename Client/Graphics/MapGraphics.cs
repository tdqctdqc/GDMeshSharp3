using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class MapGraphics : Node2D, IClientComponent
{
    public GraphicsSegmenter Segmenter { get; private set; }
    public Regime SpectatingRegime { get; private set; }
    public List<MapOverlayDrawer> Overlays { get; private set; }
    public MapOverlayDrawer DebugOverlay { get; private set; }
    public GraphicLayerHolder GraphicLayerHolder { get; private set; }
    public MapUiElements UiElements { get; private set; }
    private int _msToProcessUpdates = 50;
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }

    public MapGraphics(Client client)
    {
        var sw = new Stopwatch();
        sw.Start();

        var localPlayerRegime = client.Data
            .BaseDomain.PlayerAux
            .LocalPlayer.Regime;
        if (localPlayerRegime.Fulfilled())
        {
            SpectateRegime(localPlayerRegime.Get(client.Data));
        }
        else
        {
            SpectatingRegime = null;
        }
        
        client.Data.Notices.Player
            .PlayerChangedRegime.SubscribeForNode(n =>
            {
                if (n.Owner == client.Data.BaseDomain.PlayerAux.LocalPlayer)
                {
                    SpectateRegime(n.Owner.Regime.Get(client.Data));
                }
            }, this);

        
        Segmenter = new GraphicsSegmenter(client, 10);
        AddChild(Segmenter);
        GraphicLayerHolder = new GraphicLayerHolder(client, Segmenter, client.Data);
        Overlays = new List<MapOverlayDrawer>();
        DebugOverlay = new MapOverlayDrawer(Segmenter, (int)LayerOrder.Debug);
        Overlays.Add(DebugOverlay);
        
        UiElements = new MapUiElements(client);
        client.GraphicsLayer.AddChild(this);
        
        sw.Stop();
        client.Data.Logger.Log("map graphics setup time " + sw.Elapsed.TotalMilliseconds, LogType.Graphics);
    }
    private MapGraphics()
    {
        
    }
    public void Process(float delta)
    {
        if(Game.I.Client?.Cam() is ICameraController c)
        {
            Segmenter.Update(c.XScrollRatio);
        }
    }

    public void SpectateRegime(Regime r)
    {
        SpectatingRegime = r;
        Game.I.Client.Notices.ChangedSpectatingRegime.Invoke(r);
    }

    public MapOverlayDrawer GetOverlay(LayerOrder order)
    {
        var o = new MapOverlayDrawer(Segmenter, (int)order);
        Overlays.Add(o);
        return o;
    }

    public void RemoveOverlay(MapOverlayDrawer overlay)
    {
        Overlays.Remove(overlay);
        overlay.Clear();
    }
}