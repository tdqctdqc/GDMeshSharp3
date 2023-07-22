
public class Segment<TPrim> : ISegment<TPrim>
{
    public Segment(TPrim @from, TPrim to)
    {
        From = @from;
        To = to;
    }

    public TPrim From { get; }
    public TPrim To { get; }
    public ISegment<TPrim> ReverseGeneric()
    {
        throw new System.NotImplementedException();
    }

    public bool PointsTo(ISegment<TPrim> s)
    {
        throw new System.NotImplementedException();
    }

    public bool ComesFrom(ISegment<TPrim> s)
    {
        throw new System.NotImplementedException();
    }
}
