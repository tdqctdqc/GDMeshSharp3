using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Peep : Entity
{
    public CellRef Cell { get; private set; }
    public int Size { get; private set; }
    public PeepEmploymentReport Employment { get; private set; }

    public static Peep Create(Cell cell, ICreateWriteKey key)
    {
        var p = new Peep(PeepEmploymentReport.Construct(), 
            cell.MakeRef(), 0, 
            key.Data.IdDispenser.TakeId());
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(
        PeepEmploymentReport employment, CellRef cell,
        int size, int id) : base(id)
    {
        Employment = employment;
        Size = size;
        Cell = cell;
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
    public void SetEmploymentReport(PeepEmploymentReport peepEmployment, ProcedureWriteKey key)
    {
        Employment.Copy(peepEmployment, key);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
