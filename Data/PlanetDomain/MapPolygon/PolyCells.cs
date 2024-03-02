
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolyCells : Entity
{
    public Dictionary<int, PolyCell> Cells { get; private set; }

    public static PolyCells Create(IEnumerable<PolyCell> cells, GenWriteKey key)
    {
        var dic = cells.ToDictionary(v => v.Id, v => v);
        var e = new PolyCells(dic, key.Data.IdDispenser.TakeId());
        key.Create(e);
        return e;
    }
    [SerializationConstructor] private PolyCells(
        Dictionary<int, PolyCell> cells, int id) : base(id)
    {
        Cells = cells;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}