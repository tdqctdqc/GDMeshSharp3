
using System;
using Godot;

public class MapPos
{
    public int PolyCell { get; private set; }
    public (int DestCellId, float Proportion) Destination { get; private set; }

    public static MapPos Construct(PolyCell cell)
    {
        var mp = new MapPos(cell.Id, (-1, 0f));
        return mp;
    }
    public MapPos(int polyCell, (int DestCellId, float Proportion) destination)
    {
        PolyCell = polyCell;
        Destination = destination;
    }

    public void Set(int polyCell, (int DestCellId, float Proportion) destCell,
        MoveData moveDat, 
        LogicWriteKey key)
    {
        var cell = PlanetDomainExt.GetPolyCell(polyCell, key.Data);
        if (moveDat.MoveType.Passable(cell, moveDat.Alliance, key.Data) == false)
        {
            throw new Exception();
        }
        PolyCell = polyCell;
        Destination = destCell;
        key.Data.Context.AddToMovementRecord(moveDat.Id, this, key.Data);
    }

    public PolyCell GetCell(Data d)
    {
        return PlanetDomainExt.GetPolyCell(PolyCell, d);
    }
    public MapPos Copy()
    {
        return new MapPos(PolyCell, Destination);
    }
}