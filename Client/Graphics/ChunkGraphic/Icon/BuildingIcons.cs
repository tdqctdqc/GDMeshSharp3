using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BuildingIcons 
    : ChunkIconsMultiMesh<BuildingModel, MapBuilding>
{
    public BuildingIcons(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        Data d) 
        : base("Buildings", zoomVisibilityRange, chunk, 
            MeshExt.GetQuadMesh(Vector2.One * 25f))
    {
    }

    protected override Texture2D GetTexture(BuildingModel t)
    {
        return t.Icon.Texture;
    }

    protected override IEnumerable<MapBuilding> GetElements(Data d)
    {
        return Chunk.Polys
            .Select(p => p.GetBuildings(d))
            .Where(bs => bs != null)
            .SelectMany(bs => bs);
    }

    protected override BuildingModel GetModel(MapBuilding t, Data d)
    {
        return t.Model.Get(d);
    }

    protected override Vector2 GetWorldPos(MapBuilding t, Data d)
    {
        return t.Cell.Get(d).GetCenter();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }


    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        
        return settings;
    }
}
