using Godot;


public class PreNexus
{
    public int Id { get; private set; }
    public Vector2 Pos { get; private set; }
    public PrePoly P1 { get; private set; }
    public PrePoly P2 { get; private set; }
    public PrePoly P3 { get; private set; }
    public PreEdge E1 { get; private set; }
    public PreEdge E2 { get; private set; }
    public PreEdge E3 { get; private set; }

    public PreNexus(GenWriteKey key,
        Vector2 pos, PrePoly p1, PrePoly p2, PrePoly p3,
        PreEdge e1, PreEdge e2, PreEdge e3)
    {
        Id = key.Data.IdDispenser.TakeId();
        Pos = pos;
        P1 = p1;
        P2 = p2;
        P3 = p3;
        E1 = e1;
        E2 = e2;
        E3 = e3;
    }
}