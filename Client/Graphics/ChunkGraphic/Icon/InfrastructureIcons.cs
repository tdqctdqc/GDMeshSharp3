using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;


public partial class InfrastructureIcons : MapChunkGraphicNode<Waypoint>
{
    public InfrastructureIcons(MapChunk chunk, Data data) 
        : base(nameof(InfrastructureIcons), data, chunk)
    {
    }
    private InfrastructureIcons() : base()
    {
    }
    protected override Node2D MakeGraphic(Waypoint element, Data data)
    {
        if (element is CoastWaypoint c && c.Port)
        {
            var icon = data.Models.Infras.Port.Icon.GetMeshInstance();
            SetRelPos(icon, element.Pos, data);
            return icon;
        }

        throw new Exception();
    }

    protected override IEnumerable<Waypoint> GetKeys(Data data)
    {
        return Chunk.Polys.SelectMany(p => p.GetAssocTacWaypoints(data))
            .Distinct()
            .Where(wp => wp is CoastWaypoint c && c.Port);
    }

    protected override bool Ignore(Waypoint element, Data data)
    {
        return false;
    }
}