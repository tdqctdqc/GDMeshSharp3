using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BuildingIconLayer : MapChunkGraphicLayer<int>
{
    public BuildingIconLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(data, chunk, mg.ChunkChangedCache.BuildingsChanged)
    {
    }
    private BuildingIconLayer() : base()
    {
    }

    protected override Node2D MakeGraphic(int key, Data data)
    {
        var building = data.Society.Buildings[key];
        var icon = building.Model.Model(data).Icon.GetMeshInstance();
        SetRelPos(icon, building.Position, data);
        return icon;
    }

    protected override IEnumerable<int> GetKeys(Data data)
    {
        var keys = Chunk.Polys
            .Where(p => p.GetBuildings(data) != null)
            .SelectMany(p => p.GetBuildings(data)).Select(b => b.Id);
        return keys;
    }
}
