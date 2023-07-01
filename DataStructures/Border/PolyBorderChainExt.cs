using System;
using System.Collections.Generic;
using System.Linq;

public static class PolyBorderChainExt
{
    public static MapPolygonEdge GetEdge(this PolyBorderChain chain, Data data)
    {
        return chain.Native.Entity().GetEdge(chain.Foreign.Entity(), data);
    }
}
