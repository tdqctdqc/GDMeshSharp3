using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BuildingIcons 
    : ChunkIconsMultiMesh<BuildingModel, MapBuilding>
{
    private Vector2 _zoomVisibilityRange;
    public BuildingIcons(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        Data d) 
        : base("Buildings", chunk, MeshExt.GetQuadMesh(Vector2.One * 25f))
    {
        _zoomVisibilityRange = zoomVisibilityRange;
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
        return t.Model.Model(d);
    }

    protected override Vector2 GetWorldPos(MapBuilding t, Data d)
    {
        return PlanetDomainExt.GetPolyCell(t.PolyCellId, d).GetCenter();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }

    public override void DoUiTick(UiTickContext context, Data d)
    {
        var zoom = context.ZoomLevel;
        if (_zoomVisibilityRange.X > zoom || _zoomVisibilityRange.Y < zoom)
        {
            Visible = false;
        }
        else
        {
            Visible = true;
        }
    }
}
