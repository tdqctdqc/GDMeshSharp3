using System;
using System.Collections.Generic;
using System.Linq;

public partial class RegimeChunkModule : MapChunkGraphicModule
{
    private RegimePolyFill _fill;
    private RegimeBordersNode _borders;
    public RegimeChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(RegimeChunkModule))
    {
        _fill = new RegimePolyFill(chunk, data);
        AddNode(_fill);

        _borders = new RegimeBordersNode(chunk, data);
        AddNode(_borders);
    }

    public static ChunkGraphicLayer<RegimeChunkModule> GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<RegimeChunkModule>(
            LayerOrder.PolyFill,
            "Regimes",
            segmenter, 
            c => new RegimeChunkModule(c, client.Data), 
            client.Data);
        l.AddTransparencySetting(m => m._fill, "Fill Transparency", .25f);
        l.AddTransparencySetting(m => m._borders, "Border Transparency", 1f);
        
        
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                foreach (var kvp in l.ByChunkCoords)
                {
                    var v = kvp.Value;
                    v._fill.Update(client.Data);
                    v._borders.Draw(client.Data);
                }
            });
        });
        
        l.EnforceSettings();
        return l;
    }

}
