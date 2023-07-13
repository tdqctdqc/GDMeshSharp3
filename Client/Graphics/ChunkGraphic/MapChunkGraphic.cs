using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class MapChunkGraphic : Node2D
{
    public Dictionary<string, MapChunkGraphicModule> Modules { get; private set; }
    private MapChunk _chunk;
    private Data _data;
    public void Setup(MapGraphics mg, MapChunk chunk, Data data)
    {
        _chunk = chunk;
        _data = data;
        Position = chunk.RelTo.Center;
        Modules = new Dictionary<string, MapChunkGraphicModule>();
        Order(
            chunk, data, mg,
            AllTris,
            RegimeFill,
            Roads,
            ResourceDepositPolyFill,
            RegimeBorders,
            Icons
        );
        Init();
    }

    public void Test()
    {
        foreach (var kvp in Modules)
        {
            kvp.Value.QueueFree();
        }

        Modules = new Dictionary<string, MapChunkGraphicModule>();
    }
    public void UpdateVis()
    {
        foreach (var m in Modules.Values)
        {
            m.UpdateVis(_data);
        }
    }

    public void Init()
    {
        foreach (var module in Modules.Values)
        {
            module.Init(_data);
        }
    }
    private void Order(MapChunk chunk, Data data, MapGraphics mg, params ChunkGraphicFactory[] factories)
    {
        for (var i = 0; i < factories.Length; i++)
        {
            if (factories[i].Active == false) continue;
            var node = factories[i].GetModule(chunk, data, mg);
            node.ZIndex = i;
            Modules.Add(factories[i].Name, node);
            AddChild(node);
        }
    }
    public static ChunkGraphicFactory RegimeBorders { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(RegimeBorders), true,
            (c, d, mg) => BorderChunkGraphic.ConstructRegimeBorder(c, mg, 20f, d));

    public static ChunkGraphicFactory AllTris { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(AllTris), true,
            (c, d, mg) => new PolyTriChunkGraphic(c,d,mg));
    public static ChunkGraphicFactory Roads { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(Roads), true,
            (c, d, mg) => new RoadChunkGraphic(c, d, mg));
    public static ChunkGraphicFactory Icons { get; private set; }
        = new ChunkGraphicFactoryBasic(nameof(Icons), true,
            (c, d, mg) => new IconsChunkGraphic(c, d, mg));
    public static ChunkGraphicFactory ResourceDepositPolyFill { get; private set; }
        = new PolygonFillChunkGraphicFactory(nameof(ResourceDepositPolyFill), false, (p,d) =>
            {
                var rs = p.GetResourceDeposits(d);
                if(rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                return rs.First().Item.Model(d).Color;
            }
        );
    public static ChunkGraphicFactory RegimeFill { get; private set; }
        = new PolygonFillChunkGraphicFactory(nameof(RegimeFill), true, (p,d) =>
            {
                if (p.Regime.Fulfilled() && p.Regime.Entity(d).IsMajor) return p.Regime.Entity(d).PrimaryColor;
                return Colors.Transparent;
            }
        );
}