
using System;
using Godot;

public abstract class ChunkGraphicFactory
{
    public bool Active { get; private set; }
    public string Name { get; private set; }

    public ChunkGraphicFactory(string name, bool active)
    {
        Name = name;
        Active = active;
    }

    public abstract MapChunkGraphicModule GetModule(MapChunk c, Data d, MapGraphics mg);
}
