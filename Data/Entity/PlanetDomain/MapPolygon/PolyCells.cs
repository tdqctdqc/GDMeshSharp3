
using MessagePack;

public class PolyCells : Entity
{
    public PolyCell[] Cells { get; private set; }

    public static PolyCells Create(PolyCell[] cells, GenWriteKey key)
    {
        var e = new PolyCells(cells, key.Data.IdDispenser.TakeId());
        key.Create(e);
        return e;
    }
    [SerializationConstructor] private PolyCells(PolyCell[] cells, int id) : base(id)
    {
        Cells = cells;
    }
}