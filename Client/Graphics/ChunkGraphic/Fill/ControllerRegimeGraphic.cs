
public partial class ControllerRegimeGraphic : MapChunkGraphicModule
{
    private ControllerRegimePolyCellFill _fill;
    private ControllerPolyCellBorder _borders;
    public ControllerRegimeGraphic(MapChunk chunk, Data data) : base(chunk, nameof(OwnerRegimeGraphic))
    {
        _fill = new ControllerRegimePolyCellFill(chunk, data);
        AddNode(_fill);

        _borders = new ControllerPolyCellBorder(chunk, data);
        AddNode(_borders);
    }

    public static ChunkGraphicLayer<ControllerRegimeGraphic> GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<ControllerRegimeGraphic>(
            LayerOrder.PolyFill,
            "Owner",
            segmenter, 
            c => new ControllerRegimeGraphic(c, client.Data), 
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