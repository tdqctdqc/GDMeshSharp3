using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapChunkGraphicModule : Node2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public Dictionary<string, IMapChunkGraphicNode> Nodes { get; private set; }
    public MapChunkGraphicModule(string name)
    {
        Name = name;
        Nodes = new Dictionary<string, IMapChunkGraphicNode>();
    }

    private MapChunkGraphicModule()
    {
    }

    public void AddNode(IMapChunkGraphicNode layer)
    {
        AddChild((Node)layer);
        Nodes.Add(layer.Name, layer);
    }

    public void Init(Data data)
    {
        foreach (var kvp in Nodes)
        {
            kvp.Value.Init(data);
        }
    }

    public void Update(Data d)
    {
        foreach (var kvp in Nodes)
        {
            kvp.Value.Update(d);
        }
    }

    Node2D IMapChunkGraphicNode.Node => this;
}
