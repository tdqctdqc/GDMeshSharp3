
using System.Collections.Generic;
using Godot;

public abstract partial class ChunkGraphicMultiModule : 
    Node2D, IChunkGraphicModule
{
    public string Name { get; private set; }
    public Node2D Node => this;
    public ChunkGraphicModuleVisibility Visibility { get; }

    public ChunkGraphicMultiModule(string name, Vector2 zoomVisRange)
    {
        Name = name;
        Visibility = new ChunkGraphicModuleVisibility(zoomVisRange);
    }
    public void Draw(Data d)
    {
        foreach (var g in GetModules())
        {
            g.Draw(d);
        }
    }
    protected abstract IEnumerable<IChunkGraphicModule> GetModules();

    public void RegisterForRedraws(Data d)
    {
        foreach (var g in GetModules())
        {
            g.RegisterForRedraws(d);
        }
    }

    public abstract Settings GetSettings(Data d);
}