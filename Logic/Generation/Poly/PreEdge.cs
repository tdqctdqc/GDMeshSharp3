
public class PreEdge
{
    public int Id { get; private set; }
    public PrePoly P1 { get; private set; }
    public PrePoly P2 { get; private set; }

    public PreEdge(GenWriteKey key, PrePoly p1, PrePoly p2)
    {
        Id = key.Data.IdDispenser.TakeId();
        P1 = p1;
        P2 = p2;
    }
}