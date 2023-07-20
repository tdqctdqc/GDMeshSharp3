using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class MapChunkGraphic : Node2D
{
    public static Action<string> AddedLayer;
    public static HashSet<string> LayerNames = new HashSet<string>();
    public Dictionary<string, IMapChunkGraphicNode> Modules { get; private set; }
    private MapChunk _chunk;
    private Data _data;
    public void Setup(MapGraphics mg, MapChunk chunk, Data data)
    {
        _chunk = chunk;
        _data = data;
        Position = chunk.RelTo.Center;
        Modules = new Dictionary<string, IMapChunkGraphicNode>();
        Order(
            chunk, data, mg,
            (AllTris(data), true),
            (RegimeFill(data), true),
            (Roads(data, mg), true),
            (ResourceDepositPolyFill(data), false),
            (AllianceFill(data), true),
            (RegimeBorders(data, mg), false),
            (Icons(data, mg), true)
        );
        Init();
    }

    public void Test()
    {
        foreach (var kvp in Modules)
        {
            kvp.Value.Node.QueueFree();
        }

        Modules = new Dictionary<string, IMapChunkGraphicNode>();
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
    private void Order(MapChunk chunk, Data data, MapGraphics mg, params (IMapChunkGraphicNode, bool)[] modules)
    {
        for (var i = 0; i < modules.Length; i++)
        {
            var module = modules[i].Item1;
            module.Hidden = modules[i].Item2 == false;
            var name = module.Name;
            if (LayerNames.Contains(name) == false)
            {
                LayerNames.Add(name);
                AddedLayer.Invoke(name);
            }
            module.Node.ZIndex = i;
            Modules.Add(module.Name, module);
            AddChild(module.Node);
        }
    }

    private IMapChunkGraphicNode RegimeBorders(Data d, MapGraphics mg)
    {
        return new RegimeBorderChunkLayer(_chunk, 20f, d, mg);
    }
    private MapChunkGraphicModule AllTris(Data d)
    {
        return new PolyTriChunkGraphic(_chunk, d);
    }
    private IMapChunkGraphicNode Roads(Data d, MapGraphics mg)
    {
        return new RoadChunkGraphicLayer(_chunk, d, mg);
    }
    private IMapChunkGraphicNode Icons(Data data, MapGraphics mg)
    {
        return new IconsChunkGraphic(_chunk, data, mg);
    }
    private IMapChunkGraphicNode ResourceDepositPolyFill(Data data)
    {
        return new PolyFillLayer(nameof(ResourceDepositPolyFill), _chunk, data, p =>
        {
            var rs = p.GetResourceDeposits(data);
            if (rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
            return rs.First().Item.Model(data).Color;
        }, new Vector2(0f, 1f));
    }

    private IMapChunkGraphicNode RegimeFill(Data d)
    {
        return new PolyFillLayer(nameof(RegimeFill), _chunk, d, 
            p =>
            {
                if (p.Regime.Fulfilled() 
                    // && p.Regime.Entity(d).IsMajor
                   ) 
                    return p.Regime.Entity(d).PrimaryColor;
                return Colors.Transparent;
            },
            new Vector2(0f, 1f));
    }

    private IMapChunkGraphicNode AllianceFill(Data d)
    {
        var l = new PolyFillLayer(nameof(AllianceFill), _chunk, d, p =>
        {
            if (p.Regime.Fulfilled() == false) return Colors.Transparent;
            if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Transparent;
            if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.Empty()) return Colors.Transparent;
            var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(d);
            if (p.Regime.RefId == playerRegime.Id) return Colors.SkyBlue;
            var playerAlliance = playerRegime.GetAlliance(d);
            var polyAlliance = p.Regime.Entity(d).GetAlliance(d);

            if (playerAlliance.Members.RefIds.Contains(p.Regime.RefId)) return Colors.Green;
            if (playerAlliance.AtWar.Contains(polyAlliance)) return Colors.Red;
            if (playerAlliance.Enemies.Contains(polyAlliance)) return Colors.Orange;
            return Colors.Gray;
        }, new Vector2(0f, 1f));
        
        l.SubscribeUpdate(() => { l.Init(d); }, 
            d.Notices.Ticked.Blank, d.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        
        return l;
    }
}