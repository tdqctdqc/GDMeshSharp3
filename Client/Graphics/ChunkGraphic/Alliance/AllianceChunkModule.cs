using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceChunkModule : MapChunkGraphicModule
{
    private AllianceFillNode _fill;
    // private AllianceBorderNode _allianceBorder;
    // private BorderChunkNode _regimeBorder;
    
    public AllianceChunkModule(MapChunk chunk, Data data) : base(nameof(AllianceChunkModule))
    {
        _fill = new AllianceFillNode(chunk, data);
        AddNode(_fill);

        data.BaseDomain.PlayerAux.PlayerChangedRegime
            .SubscribeForNode(n => HandlePlayerRegimeChange(n, data), this);
        // _allianceBorder = new AllianceBorderNode(chunk, 30f, data);
        // AddNode(_allianceBorder);
        //
        // _regimeBorder = new BorderChunkNode(nameof(Alliance), chunk, 
        //     p => p.Regime.RefId,
        //     p => p.Regime.Fulfilled() 
        //         ? p.Regime.Entity(data).GetAlliance(data).Leader.Entity(data).PrimaryColor 
        //         : Colors.Transparent,
        //     5f, data);
        // AddNode(_regimeBorder);
    }
    
    public static ChunkGraphicLayer<AllianceChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<AllianceChunkModule>(
            "Alliances",
            segmenter, 
            c => new AllianceChunkModule(c, d), 
            d);
        l.RegisterForNotice(d.Planet.PolygonAux.ChangedRegime,
            r => r.Entity.GetChunk(d),
            (n, m) => { m.HandlePolygonRegimeChange(n); });
        return l;
    }

    private void HandlePlayerRegimeChange(ValChangeNotice<Player, Regime> n, Data d)
    {
        var player = n.Entity;
        if (player != d.BaseDomain.PlayerAux.LocalPlayer) return;
        _fill.Redraw(d);
    }
    private void HandlePolygonRegimeChange(ValChangeNotice<MapPolygon, Regime> notice)
    {
        _fill.Updates.Add(notice.Entity);
    }
    private void HandleTurn(Data d)
    {
        _fill.Redraw(d);
    }
}
