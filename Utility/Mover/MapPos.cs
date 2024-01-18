
using System;
using Godot;

public class MapPos
{
    public int PolyCell { get; private set; }
    public (int DestCellId, float proportion) DestCell { get; private set; }

    public static MapPos Construct(PolyCell cell)
    {
        var mp = new MapPos(cell.Id, (-1, 0f));
        return mp;
    }
    public MapPos(int polyCell, (int DestCellId, float proportion) destCell)
    {
        PolyCell = polyCell;
        DestCell = destCell;
    }

    public void Set(int polyCell, (int DestCellId, float proportion) destCell,
        MoveData moveDat, 
        LogicWriteKey key)
    {
        PolyCell = polyCell;
        DestCell = destCell;
        key.Data.Context.AddToMovementRecord(moveDat.Id, this, key.Data);
    }

    public PolyCell GetCell(Data d)
    {
        return PlanetDomainExt.GetPolyCell(PolyCell, d);
    }
    public MapPos Copy()
    {
        return new MapPos(PolyCell, DestCell);
    }
}