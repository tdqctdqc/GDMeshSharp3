
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolyCells : Entity
{
    public Dictionary<int, Cell> Cells { get; private set; }

    public static PolyCells Create(Dictionary<int, Cell> cells, GenWriteKey key)
    {
        var e = new PolyCells(cells, key.Data.IdDispenser.TakeId());
        key.Create(e);
        return e;
    }
    [SerializationConstructor] private PolyCells(
        Dictionary<int, Cell> cells, int id) : base(id)
    {
        Cells = cells;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}