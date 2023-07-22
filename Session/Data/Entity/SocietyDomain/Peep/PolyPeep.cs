using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolyPeep : Entity
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    public int Size { get; private set; }

    public static PolyPeep Create(MapPolygon poly, CreateWriteKey key)
    {
        var p = new PolyPeep(poly.MakeRef(), 0, 
            key.IdDispenser.GetID());
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private PolyPeep(EntityRef<MapPolygon> poly,
        int size, int id) : base(id)
    {
        Size = size;
        Poly = poly;
    }

    public void GrowSize(int delta, ProcedureWriteKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        Size += delta;
    }
    public void GrowSize(int delta, GenWriteKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        Size += delta;
    }

    public void ShrinkSize(int delta, ProcedureWriteKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        Size -= delta;
    }

    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
}
