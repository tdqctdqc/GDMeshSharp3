
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ResourceExtractionIcons
    : ChunkIconsMultiMesh<ResourceExtractionBuilding, ResourceDeposit>
{
    public ResourceExtractionIcons(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        Data d) 
        : base("Resource ExtractionBuildings", zoomVisibilityRange,
            chunk, MeshExt.GetQuadMesh(Vector2.One * 20f))
    {
        ZIndex = (int)LayerOrder.Icons;
    }


    protected override Texture2D GetTexture(ResourceExtractionBuilding t)
    {
        return t.Icon.Texture;
    }

    protected override IEnumerable<ResourceDeposit> GetElements(Data d)
    {
        return Chunk.Cells.Where(c => c.GetResourceDeposit(d)
            is ResourceDeposit rd && rd.Extraction.Fulfilled())
            .Select(c => c.GetResourceDeposit(d));
    }

    protected override ResourceExtractionBuilding GetModel(ResourceDeposit t, Data d)
    {
        return t.Extraction.Get(d);
    }

    protected override Vector2 GetWorldPos(ResourceDeposit t, Data d)
    {
        return t.Cell.Get(d).GetCenter();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnJustTicked(d);
    }

    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        
        return settings;
    }
}