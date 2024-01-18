using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ConstructionIcons : MapChunkGraphicNode<Construction>
{
    public ConstructionIcons(MapChunk chunk, Data data) 
        : base(nameof(ConstructionIcons), data, chunk)
    {        
    }
    private ConstructionIcons() : base()
    {
    }
    protected override Node2D MakeGraphic(Construction element, Data data)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var construction = element;
        var icon = construction.Model.Model(data).Icon.GetMeshInstance(size);
        var constrSignMesh = data.Models.Flows.ConstructionCap.Icon.GetMeshInstance(size);
        icon.AddChild(constrSignMesh);
        var cell = PlanetDomainExt.GetPolyCell(construction.PolyCellId, data);
        SetRelPos(icon, cell.GetCenter(), data);
        return icon;
    }

    protected override IEnumerable<Construction> GetKeys(Data data)
    {
        return Chunk.Polys
            .Where(p => data.Infrastructure.CurrentConstruction.ByPoly.ContainsKey(p.Id))
            .SelectMany(p => data.Infrastructure.CurrentConstruction.ByPoly[p.Id]);
    }

    protected override bool Ignore(Construction element, Data data)
    {
        return false;
    }
}
