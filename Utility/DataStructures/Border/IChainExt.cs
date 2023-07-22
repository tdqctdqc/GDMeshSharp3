using System;
using System.Collections.Generic;
using System.Linq;

public static class IChainExt
{
    public static IEnumerable<TSeg> Ordered<TSeg, TPrim>(this IEnumerable<IChain<TSeg, TPrim>> borders)
        where TSeg : ISegment<TPrim>
    {
        return borders.Ordered<IChain<TSeg, TPrim>, TPrim>().SelectMany(b => b.Segments);
    }
}