using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DiplomacyChunkModule : MapChunkGraphicModule
{
    private DiplomacyPolyFill _diplomacyFill;
    private AllianceBordersGraphic _allianceBorder;
    
    public DiplomacyChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(DiplomacyChunkModule))
    {
        _diplomacyFill = new DiplomacyPolyFill(chunk, data);
        AddNode(_diplomacyFill);

        _allianceBorder = new AllianceBordersGraphic(chunk, data, true);
        AddNode(_allianceBorder);
        
        
    }
    
    public static ChunkGraphicLayer<DiplomacyChunkModule> 
        GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<DiplomacyChunkModule>(
            LayerOrder.PolyFill,
            "Diplomacy",
            segmenter, 
            c => new DiplomacyChunkModule(c, client.Data), 
            client.Data);
        l.AddTransparencySetting(m => m._diplomacyFill, "Transparency");
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                foreach (var kvp in l.ByChunkCoords)
                {
                    var v = kvp.Value;
                    v._diplomacyFill.Update(client.Data);
                    v._allianceBorder.Draw(client.Data);
                }
            });
        });
        client.Data.BaseDomain.PlayerAux.PlayerChangedRegime.Subscribe(n =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                foreach (var kvp in l.ByChunkCoords)
                {
                    var v = kvp.Value;
                    v._diplomacyFill.Update(client.Data);
                    v._allianceBorder.Draw(client.Data);
                }
            });
        });
        
        l.EnforceSettings();
        return l;
    }

    
}
