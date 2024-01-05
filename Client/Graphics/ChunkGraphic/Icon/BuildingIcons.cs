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

    protected override Node2D MakeGraphic(MapBuilding element, Data data)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var icon = element.Model.Model(data).Icon.GetMeshInstance(size);
        SetRelPos(icon, element.Position, data);
        return icon;
    }

    protected override IEnumerable<MapBuilding> GetKeys(Data data)
    {
        var keys = Chunk.Polys
            .Where(p => p.GetBuildings(data) != null)
            .SelectMany(p => p.GetBuildings(data));
        return keys;
    }

    protected override bool Ignore(MapBuilding element, Data data)
    {
        return false;
    }
}
