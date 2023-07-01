using System;
using System.Collections.Generic;
using System.Linq;

public class ChunkGraphicFactoryBasic : ChunkGraphicFactory
{
    private Func<MapChunk, Data, MapGraphics, MapChunkGraphicModule> _func;
    public ChunkGraphicFactoryBasic(string name, bool active,
        Func<MapChunk, Data, MapGraphics, MapChunkGraphicModule> func) 
        : base(name, active)
    {
        _func = func;
    }

    public override MapChunkGraphicModule GetModule(MapChunk c, Data d, MapGraphics mg)
    {
        return _func(c, d, mg);
    }
}
