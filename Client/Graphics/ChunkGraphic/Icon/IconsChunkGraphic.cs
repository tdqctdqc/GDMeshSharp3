using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class IconsChunkGraphic : MapChunkGraphicModule
{
    public IconsChunkGraphic(MapChunk chunk, Data data, MapGraphics mg)
    {
        var constructions = new ConstructionIconLayer(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), constructions);

        var settlements = new SettlementIconLayer(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), settlements);
        
        var buildings = new BuildingIconLayer(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), buildings);
        
        // var regimeIcons = new RegimeFlagChunkLayer(chunk, data, mg);
        // AddLayer(new Vector2(.4f, .8f), regimeIcons);
    }

    private IconsChunkGraphic()
    {
    }
}
