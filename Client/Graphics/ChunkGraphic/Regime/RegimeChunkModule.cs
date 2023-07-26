using System;
using System.Collections.Generic;
using System.Linq;

public partial class RegimeChunkModule : MapChunkGraphicModule
{
    private RegimeFillNode _fill;
    // private RegimeBordersNode _borders;
    public RegimeChunkModule(MapChunk chunk, Data data) : base(nameof(RegimeChunkModule))
    {
        _fill = new RegimeFillNode(chunk, data);
        AddNode(_fill);

        // _borders = new RegimeBordersNode(chunk, p => p.Regime.RefId, 20f, data);
        // AddNode(_borders);
    }

    public static ChunkGraphicLayer<RegimeChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<RegimeChunkModule>(
            "Regimes",
            segmenter, 
            c => new RegimeChunkModule(c, d), 
            d);
        l.RegisterForNotice(d.Planet.PolygonAux.ChangedRegime,
            r => ((MapPolygon) r.Entity).GetChunk(d),
            (n, m) => { m.HandlePolygonRegimeChange(n); });
        return l;
    }

    private void HandlePolygonRegimeChange(ValChangeNotice<MapPolygon, Regime> notice)
    {
        _fill.Updates.Add(notice.Entity);
    }
}
