using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BuildingIcons : MapChunkGraphicNode<MapBuilding>
{
    public BuildingIcons(MapChunk chunk, Data data) 
        : base(nameof(BuildingIcons), data, chunk)
    {
    }
    private BuildingIcons() : base()
    {
    }

    protected override Node2D MakeGraphic(MapBuilding settlement, Data data)
    {
        var icon = settlement.Model.Model(data).Icon.GetMeshInstance();
        SetRelPos(icon, settlement.Position, data);
        return icon;
    }

    protected override IEnumerable<MapBuilding> GetKeys(Data data)
    {
        var keys = Chunk.Polys
            .Where(p => p.GetBuildings(data) != null)
            .SelectMany(p => p.GetBuildings(data));
        return keys;
    }
}
