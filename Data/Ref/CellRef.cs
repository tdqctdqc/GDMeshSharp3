
public struct CellRef : IDRef<Cell>
{
    public int RefId { get; }

    public CellRef(int refId)
    {
        RefId = refId;
    }

    public Cell Get(Data d)
    {
        return PlanetDomainExt.GetPolyCell(RefId, d);
    }
}