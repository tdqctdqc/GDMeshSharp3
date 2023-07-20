using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class IconsChunkGraphic : MapChunkGraphicModule
{
    public IconsChunkGraphic(MapChunk chunk, Data data, MapGraphics mg) : base(nameof(IconsChunkGraphic))
    {
        var constructions = new ConstructionIconLayer(chunk, data, mg);
        AddLayer(constructions);

        var settlements = new SettlementIconLayer(chunk, data, mg);
        AddLayer(settlements);
        
        var buildings = new BuildingIconLayer(chunk, data, mg);
        AddLayer(buildings);
    }
}
