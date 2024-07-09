
public struct CellRef : IdRef, IdRef<Cell>
{
    public int RefId { get; }

    public CellRef()
    {
        RefId = -1;
    }
    public CellRef(int refId)
    {
        RefId = refId;
    }
    IIdentifiable IdRef.Get(Data data) => Get(data);
    public Cell Get(Data d)
    {
        return PlanetDomainExt.GetPolyCell(RefId, d);
    }
}