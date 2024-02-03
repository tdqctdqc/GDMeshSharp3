using System;
using System.Collections.Generic;
using System.Linq;

public partial class OwnerRegimeGraphic : MapChunkGraphicModule
{
    private RegimePolyFill _fill;
    private RegimeBordersGraphic _borders;
    public OwnerRegimeGraphic(MapChunk chunk, Data data) : base(chunk, nameof(OwnerRegimeGraphic))
    {
        _fill = new RegimePolyFill(chunk, data);
        AddNode(_fill);

        _borders = new RegimeBordersGraphic(chunk, data);
        AddNode(_borders);
    }

    public static ChunkGraphicLayer<OwnerRegimeGraphic> GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<OwnerRegimeGraphic>(
            LayerOrder.PolyFill,
            "Owner",
            segmenter, 
            c => new OwnerRegimeGraphic(c, client.Data), 
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
