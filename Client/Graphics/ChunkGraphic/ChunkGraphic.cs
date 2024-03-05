using System.Collections.Generic;
using Godot;

public partial class ChunkGraphic : Node2D
{
    public MapChunk Chunk { get; private set; }
    public static Vector2 IconZoomVisRange { get; private set; }
        = new Vector2(0f, .5f);
    public TerrainChunkModule Terrain {get; private set; }
    public RoadChunkGraphicNode Roads {get; private set; }
    public IconsChunkModule Icons {get; private set; }
    public ResourceIcons ResourceIcons {get; private set; }
    public ChunkUnitsGraphic Units { get; private set; }
    
    public ChunkGraphic(MapChunk chunk, GraphicLayerHolder holder,
        Data d)
    {
        ZAsRelative = false;
        Chunk = chunk;
        MakeModules(holder, d);
        foreach (var g in GetModules())
        {
            g.RegisterForRedraws(d);
            AddChild(g.Node);
        }
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

    public void DoUiTick(UiTickContext context, Data d)
    {
        foreach (var g in GetModules())
        {
            g.DoUiTick(context, d);
        }
    }
    public void Draw(Data d)
    {
        foreach (var g in GetModules())
        {
            g.Draw(d);
        }
    }

    private void MakeModules(GraphicLayerHolder holder, Data d)
    {
        Terrain = new TerrainChunkModule(Chunk, d);
        Roads = new RoadChunkGraphicNode(Chunk, ChunkGraphic.IconZoomVisRange, d);
        Icons = new IconsChunkModule(Chunk, d);
        ResourceIcons = new ResourceIcons(Chunk, ChunkGraphic.IconZoomVisRange, d);
        Units = new ChunkUnitsGraphic(Chunk, IconZoomVisRange,
            holder.UnitGraphics,
            d);
    }
    private IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return Terrain;
        yield return Roads;
        yield return Icons;
        yield return ResourceIcons;
        yield return Units;
    }
}