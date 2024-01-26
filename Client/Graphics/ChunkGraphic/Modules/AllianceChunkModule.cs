using System;
using System.Collections.Generic;
using System.Linq;

public partial class AllianceChunkModule : MapChunkGraphicModule
{
    private AlliancePolyFill _fill;
    private AllianceBordersGraphic _borders;

    public AllianceChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(AllianceChunkModule))
    {
        _fill = new AlliancePolyFill(chunk, data);
        AddNode(_fill);
        _borders = new AllianceBordersGraphic(chunk, data, false);
        AddNode(_borders);
    }
    
    public static ChunkGraphicLayer<AllianceChunkModule> GetLayer(
        Client client, 
        GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<AllianceChunkModule>(
            LayerOrder.PolyFill,
            "Alliances",
            segmenter, 
            c => new AllianceChunkModule(c, client.Data), 
            client.Data);
        l.AddTransparencySetting(m => m._fill, "Fill Transparency");
        l.AddTransparencySetting(m => m._borders, "Border Transparency");
        
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
