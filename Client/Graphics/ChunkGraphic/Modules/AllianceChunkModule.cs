using System;
using System.Collections.Generic;
using System.Linq;

public partial class AllianceChunkModule : MapChunkGraphicModule
{
    private AlliancePolyFill _fill;
    private AllianceBordersNode _borders;

    public AllianceChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(AllianceChunkModule))
    {
        _fill = new AlliancePolyFill(chunk, data);
        AddNode(_fill);
        _borders = new AllianceBordersNode(chunk, data, false);
        AddNode(_borders);
        
        data.Society.AllianceAux.AllianceAddedRegime
            .SubscribeForNode(n => HandleRegimeAllianceChange(n.Item2, n.Item1, data), this);
        data.Society.AllianceAux.AllianceRemovedRegime
            .SubscribeForNode(n => HandleRegimeAllianceChange(n.Item2, n.Item1, data), this);
    }
    
    public static ChunkGraphicLayer<AllianceChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<AllianceChunkModule>(
            LayerOrder.PolyFill,
            "Alliances",
            segmenter, 
            c => new AllianceChunkModule(c, d), 
            d);
        l.AddTransparencySetting(m => m._fill, "Fill Transparency");
        l.AddTransparencySetting(m => m._borders, "Border Transparency");
        
        l.RegisterForChunkNotice(d.Planet.PolygonAux.ChangedOwnerRegime,
            r =>
            {
                return r.Entity.GetChunkAndNeighboringChunks(d);
            },
            (n, m) => { m.HandlePolygonRegimeChange(n, d); });
        return l;
    }
    
    private void HandleRegimeAllianceChange(Regime regime, Alliance newA, Data d)
    {
        var relevant = Chunk.Polys.Union(Chunk.Bordering)
            .Where(p => p.OwnerRegime.RefId == regime.Id);
        if (relevant.Count() == 0) return;
        foreach (var p in relevant)
        {
            if(Chunk.Polys.Contains(p)) _fill.Updates.Add(p);
        }
        _borders.QueueChangeAll();
    }
    private void HandlePolygonRegimeChange(ValChangeNotice<MapPolygon, Regime> notice, Data data)
    {
        _fill.Updates.Add(notice.Entity);
        _borders.QueueChangeAll();
    }
}
