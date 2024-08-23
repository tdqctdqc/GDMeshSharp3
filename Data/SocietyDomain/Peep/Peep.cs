using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Peep : Entity
{
    public CellRef Cell { get; private set; }
    public float Size { get; private set; }
    public PeepEmploymentReport Employment { get; private set; }

    public static Peep Create(Cell cell, GenKey key)
    {
        var p = new Peep(PeepEmploymentReport.Construct(), 
            cell.MakeRef(), 0, 
            key.Data.IdDispenser.TakeId());
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(
        PeepEmploymentReport employment, CellRef cell,
        float size, int id) : base(id)
    {
        Employment = employment;
        Size = size;
        Cell = cell;
    }

    public void GrowSize(float delta, ProcedureKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        Size += delta;
    }
    public void GrowSize(float delta, GenKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        Size += delta;
    }

    public void ShrinkSize(float delta, ProcedureKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        Size -= delta;
    }
    public void SetEmploymentReport(PeepEmploymentReport peepEmployment, ProcedureKey key)
    {
        Employment.Copy(peepEmployment, key);
    }

    public override void CleanUp(IWriteKey key)
    {
        
    }
}
