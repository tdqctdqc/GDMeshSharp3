
using System.Collections.Generic;

public interface IChain<TSegment, TPrim> : 
    IChain<TSegment>,
    ISegment<TPrim> where TSegment : ISegment<TPrim>
{
}

public interface IChain<TSegment> 
{
    IReadOnlyList<TSegment> Segments { get; }
}


