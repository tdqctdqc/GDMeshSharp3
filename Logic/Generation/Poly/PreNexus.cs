using Godot;


public class PreNexus
{
    public int Id { get; private set; }
    public Vector2 Pos { get; private set; }
    public PrePoly P1 { get; private set; }
    public PrePoly P2 { get; private set; }
    public PrePoly P3 { get; private set; }

    public PreNexus(GenWriteKey key,
        Vector2 pos, PrePoly p1, PrePoly p2, PrePoly p3)
    {
        Id = key.Data.IdDispenser.TakeId();
        Pos = pos;
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }
}