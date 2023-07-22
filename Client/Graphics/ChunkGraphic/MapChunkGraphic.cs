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
            (Regime(data, mg), true),
            (Roads(data, mg), true),
            (ResourceDepositPolyFill(data), false),
            (Alliance(data, mg), true),
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

    private IMapChunkGraphicNode Regime(Data d, MapGraphics mg)
    {
        var module = new MapChunkGraphicModule("Regime");
        var fill = new PolyFillLayer(nameof(Regime), _chunk, d, 
            p =>
            {
                if (p.Regime.Fulfilled() 
                    // && p.Regime.Entity(d).IsMajor
                   ) 
                    return p.Regime.Entity(d).PrimaryColor;
                return Colors.Transparent;
            },
            new Vector2(0f, 1f));
        var borders = new BorderChunkLayer("Borders", _chunk, p => p.Regime.RefId,
            p => p.Regime.Fulfilled() ? p.Regime.Entity(d).SecondaryColor : Colors.Transparent,
            20f, d, mg);
        module.AddLayer(fill);
        module.AddLayer(borders);
        return module;
    }

    private IMapChunkGraphicNode Alliance(Data d, MapGraphics mg)
    {
        var module = new MapChunkGraphicModule("Alliance");
        
        var fill = new PolyFillLayer("fill", _chunk, d, p =>
        {
            if (p.Regime.Fulfilled() == false) return Colors.Transparent;
            if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Transparent;
            if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.Empty()) return Colors.Transparent;
            var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(d);
            if (p.Regime.RefId == playerRegime.Id) return Colors.SkyBlue;
            var playerAlliance = playerRegime.GetAlliance(d);
            var polyAlliance = p.Regime.Entity(d).GetAlliance(d);

            if (playerAlliance.Members.RefIds.Contains(p.Regime.RefId)) 
                return Colors.Green.GetPeriodicShade(p.Regime.RefId);
            if (playerAlliance.AtWar.Contains(polyAlliance)) 
                return Colors.Red;
            if (playerAlliance.Rivals.Contains(polyAlliance)) 
                return Colors.Orange;
            return Colors.Gray;
        }, new Vector2(0f, 1f));
        
        var allianceBorders = new BorderChunkLayer(nameof(Alliance), _chunk, 
            p => p.Regime.Fulfilled() ? p.Regime.Entity(d).GetAlliance(d).Id : -1,
            p => p.Regime.Fulfilled() 
                ? p.Regime.Entity(d).GetAlliance(d).Leader.Entity(d).PrimaryColor 
                : Colors.Transparent,
            30f, d, mg);
        
        var regimeBorders = new BorderChunkLayer(nameof(Alliance), _chunk, 
            p => p.Regime.RefId,
            p => p.Regime.Fulfilled() 
                ? p.Regime.Entity(d).GetAlliance(d).Leader.Entity(d).PrimaryColor 
                : Colors.Transparent,
            5f, d, mg);
        module.AddLayer(fill);
        module.AddLayer(regimeBorders);
        module.AddLayer(allianceBorders);
        module.SubscribeUpdate(() => { module.Init(d); }, 
            d.Notices.Ticked.Blank, d.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        return module;
    }
}