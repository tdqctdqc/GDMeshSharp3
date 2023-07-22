
public interface IBorderChain<TSegment, TPrim, TRegion> 
    : IBorder<TRegion>, IChain<TSegment, TPrim> where TSegment : ISegment<TPrim>
{
}
