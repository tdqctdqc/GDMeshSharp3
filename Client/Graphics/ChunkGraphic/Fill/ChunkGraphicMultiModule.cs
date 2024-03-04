
using System.Collections.Generic;
using Godot;

public abstract partial class ChunkGraphicMultiModule : 
    Node2D, IChunkGraphicModule
{
    public Node2D Node => this;

    public ChunkGraphicMultiModule()
    {
        
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
}