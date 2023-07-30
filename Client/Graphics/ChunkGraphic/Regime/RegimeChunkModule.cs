using System;
using System.Collections.Generic;
using System.Linq;

public partial class RegimeChunkModule : MapChunkGraphicModule
{
    private RegimeFillNode _fill;
    private RegimeBordersNode _borders;
    public RegimeChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(RegimeChunkModule))
    {
        _fill = new RegimeFillNode(chunk, data);
        AddNode(_fill);

        _borders = new RegimeBordersNode(chunk, 20f, data);
        AddNode(_borders);
    }

    public static ChunkGraphicLayer<RegimeChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<RegimeChunkModule>(
            "Regimes",
            segmenter, 
            c => new RegimeChunkModule(c, d), 
            d);
        l.RegisterForChunkNotice(d.Planet.PolygonAux.ChangedRegime,
            r => ((MapPolygon) r.Entity).GetChunk(d),
            (n, m) => { m.HandlePolygonRegimeChange(n, d); });
        return l;
    }

    private void HandlePolygonRegimeChange(ValChangeNotice<MapPolygon, Regime> notice, Data data)
    {
        _fill.Updates.Add(notice.Entity);
        _borders.QueueChangeAround(notice.Entity, data);
    }
}
