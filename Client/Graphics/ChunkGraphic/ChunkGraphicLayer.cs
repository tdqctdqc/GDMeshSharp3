using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkGraphicLayer<TGraphic> : IGraphicLayer
    where TGraphic : Node2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public Dictionary<Vector2, TGraphic> ByChunkCoords { get; private set; }
    public int Z { get; }
    public List<ISettingsOption> Settings { get; private set; }
    private Dictionary<ISettingsOption, Action<TGraphic>> _settingsUpdaters;
    private Func<MapChunk, TGraphic> _getGraphic;
    private bool _visible = true;
    private GraphicsSegmenter _segmenter;
    public ChunkGraphicLayer(int z, string name, GraphicsSegmenter segmenter,
        Func<MapChunk, TGraphic> getGraphic, Data data)
    {
        Z = z;
        _getGraphic = getGraphic;
        Name = name;
        Settings = new List<ISettingsOption>();
        _settingsUpdaters = new Dictionary<ISettingsOption, Action<TGraphic>>();
        _segmenter = segmenter;
        ByChunkCoords = new Dictionary<Vector2, TGraphic>();
        foreach (var chunk in data.Planet.PolygonAux.Chunks)
        {
            Add(chunk, data);
        }
    }

    private void Add(MapChunk chunk, Data data)
    {
        var graphic = _getGraphic(chunk);
        graphic.ZIndex = Z;
        graphic.ZAsRelative = false;
        ByChunkCoords.Add(chunk.Coords, graphic);
        _segmenter.AddElement(graphic, chunk.RelTo.Center);
        foreach (var kvp in _settingsUpdaters)
        {
            kvp.Value.Invoke(graphic);
        }
    }

    public void EnforceSettings()
    {
        foreach (var graphic in ByChunkCoords.Values)
        {
            foreach (var kvp in _settingsUpdaters)
            {
                kvp.Value.Invoke(graphic);
            }
        }
    }
    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            foreach (var graphic in ByChunkCoords.Values)
            {
                graphic.Visible = _visible;
            }
        } 
    }
    public Control GetControl()
    {
        var button = ButtonExt.GetButton();
        Action<bool> setButtonText = v => button.Text = $"{(v ? "Showing" : "Hiding")} {Name}";
        setButtonText(Visible);
        button.ButtonUp += () =>
        {
            var visible = Visible == false;
            setButtonText(visible);
            Visible = visible;
        };
        return button;
    }
    public void RegisterForChunkNotice<TNotice>(RefAction<TNotice> refAction,
        Func<TNotice, IEnumerable<MapChunk>> getChunks, Action<TNotice, TGraphic> queueUpdate)
    {
        refAction.SubscribeForNode(n =>
        {
            foreach (var chunk in getChunks(n))
            {
                var graphic = ByChunkCoords[chunk.Coords];
                queueUpdate(n, graphic);
            }
        }, _segmenter);
    }

    public void AddSetting<T>(SettingsOption<T> option, 
        Action<TGraphic, T> update)
    {
        option.SettingChanged.SubscribeForNode(() =>
        {
            foreach (var g in ByChunkCoords.Values)
            {
                update(g, option.Value);
            }
        }, _segmenter);
        Settings.Add(option);
        _settingsUpdaters.Add(option, g => update(g, option.Value));
    }
    public void AddTransparencySetting(Func<TGraphic, Node2D> getNode, string label)
    {
        var option = new FloatSettingsOption(label, 1f, 0f, 1f, .05f, false);
        AddSetting(option, (module, value) => getNode(module).Modulate = new Color(Colors.White, option.Value));
    }
}

public static class MapChunkGraphicNodeExt
{
    public static void RegisterForEntityLifetime<TEntity, TGraphic>(
        this ChunkGraphicLayer<TGraphic> l, 
        Func<TEntity, MapChunk> getChunk, Func<TGraphic,  MapChunkGraphicNode<TEntity>> getNode, Data d) 
        where TGraphic : Node2D, IMapChunkGraphicNode where TEntity : Entity
    {
        l.RegisterForChunkNotice(d.GetEntityTypeNode<TEntity>().Created, 
            n => getChunk((TEntity)n.Entity).Yield(),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueAdd((TEntity)n.Entity);
            });
        
        l.RegisterForChunkNotice(d.GetEntityTypeNode<TEntity>().Destroyed, 
            n => getChunk((TEntity)n.Entity).Yield(),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueRemove((TEntity)n.Entity);
            });
    }
    
    public static void RegisterForAdd<TKey, TGraphic>(this ChunkGraphicLayer<TGraphic> l,
        RefAction<TKey> action, 
        Func<TKey, MapChunk> getChunk, 
        Func<TGraphic, MapChunkGraphicNode<TKey>> getNode) 
        where TGraphic : Node2D, IMapChunkGraphicNode
    {
        l.RegisterForChunkNotice(action, 
            n => getChunk(n).Yield(),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueAdd(n);
            });
    }
    
    public static void RegisterForRemove<TKey, TGraphic>(this ChunkGraphicLayer<TGraphic> l,
        RefAction<TKey> action, 
        Func<TKey, MapChunk> getChunk, 
        Func<TGraphic, MapChunkGraphicNode<TKey>> getNode) 
        where TGraphic : Node2D, IMapChunkGraphicNode
    {
        l.RegisterForChunkNotice(action, 
            n => getChunk(n).Yield(),
            (n, graphic) =>
            {
                var node = getNode(graphic);
                node.QueueRemove(n);
            });
    }
}