
using System;
using Godot;

public partial class ControllerRegimePolyCellFill 
    : PolyCellFillChunkGraphic
{
    public ControllerRegimePolyCellFill(MapChunk chunk, Data data) 
        : base("Controller", chunk,
            (c, d) =>
            {
                var r = c.Controller.Entity(d);
                if(r == null) return Colors.Transparent;
                return c.Controller.Entity(d).GetMapColor();
            },
            data)
    {
    }
    
    public static ChunkGraphicLayer<ControllerRegimePolyCellFill> 
        GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<ControllerRegimePolyCellFill>(
            LayerOrder.PolyFill,
            "Controller",
            segmenter, 
            c => new ControllerRegimePolyCellFill(c, client.Data), 
            client.Data);
        l.AddTransparencySetting(m => m, "Fill Transparency", .25f);
        
        
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                foreach (var kvp in l.ByChunkCoords)
                {
                    var v = kvp.Value;
                    v.Update(client.Data);
                }
            });
        });
        
        l.EnforceSettings();
        return l;
    }
}