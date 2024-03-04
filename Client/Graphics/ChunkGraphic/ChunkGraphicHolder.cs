using System.Collections.Generic;
using Godot;

public partial class ChunkGraphicHolder : Node2D
{
    public MapChunk Chunk { get; private set; }
    public List<IChunkGraphicModule> Graphics { get; private set; }
    public ChunkGraphicHolder(MapChunk chunk, Data d)
    {
        Chunk = chunk;
        Graphics = GetModules(d);
        Graphics.ForEach(g =>
        {
            g.RegisterForRedraws(d);
            AddChild(g.Node);
        });
    }

    public override void _Process(double delta)
    {
        var cam = Game.I.Client.Cam();
        if (cam.InViewport(Chunk.RelTo.Center) == false)
        {
            Visible = false;
        }
        else
        {
            Visible = true;
        }
    }

    public void Draw(Data d)
    {
        foreach (var g in Graphics)
        {
            g.Draw(d);
        }
    }
    private List<IChunkGraphicModule> GetModules(Data d)
    {
        return new List<IChunkGraphicModule>()
        {
            new TerrainChunkModule(Chunk, d),
            new PoliticalChunkModule(Chunk, d),
            new RoadChunkGraphicNode(Chunk, d),
            new IconsChunkModule(Chunk, d),
            new ResourceIcons(Chunk, d),
        };
    }
}