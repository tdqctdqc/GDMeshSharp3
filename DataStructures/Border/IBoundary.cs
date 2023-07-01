
    using System;
    using System.Collections.Generic;

    public interface IBoundary<TPrim> : IChain<Segment<TPrim>, TPrim>
    {
        Action<TPrim> CrossedSelf { get; set; }
        IReadOnlyList<BorderEdge<TPrim>> OrderedBorderPairs { get; }
        HashSet<TPrim> Elements { get; }
    }
