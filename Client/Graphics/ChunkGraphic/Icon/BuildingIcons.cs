using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BuildingIcons 
    : ChunkIconsMultiMesh<BuildingModel, MapBuilding>
{
    public BuildingIcons(MapChunk chunk, Data d) 
        : base("Buildings", chunk, MeshExt.GetQuadMesh(Vector2.One * 25f)) 
    {
        Draw(d);
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
}
