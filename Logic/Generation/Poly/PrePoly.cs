using System.Collections.Generic;
using Godot;


public class PrePoly : IIdentifiable
{
    public int Id { get; private set; }
    public Vector2 RelTo { get; private set; }
    public List<PreCell> Cells { get; private set; }
    public HashSet<PrePoly> Neighbors { get; set; }
    
    public PrePoly(GenWriteKey key, Vector2 relTo)
    {
        Id = key.Data.IdDispenser.TakeId();
        Cells = new List<PreCell>();
        Neighbors = new HashSet<PrePoly>();
        RelTo = relTo;
    }
}