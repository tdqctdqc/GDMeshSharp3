using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapChunkGraphicModule : Node2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public MapChunk Chunk { get; private set; }

    public Dictionary<string, IMapChunkGraphicNode> Nodes { get; private set; }
    public MapChunkGraphicModule(MapChunk chunk, string name)
    {
        Chunk = chunk;
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
    Node2D IMapChunkGraphicNode.Node => this;
}
