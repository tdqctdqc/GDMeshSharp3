
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class CellHolder : Entity
{
    public Dictionary<int, Cell> Cells { get; private set; }

    public static CellHolder Create(Dictionary<int, Cell> cells, GenWriteKey key)
    {
        var e = new CellHolder(cells, key.Data.IdDispenser.TakeId());
        key.Create(e);
        return e;
    }
    [SerializationConstructor] private CellHolder(
        Dictionary<int, Cell> cells, int id) : base(id)
    {
        Cells = cells;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}