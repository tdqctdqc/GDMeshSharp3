
using System;
using Godot;

public class MapPos
{
    public int PolyCell { get; private set; }
    public (int DestCellId, float Proportion) Destination { get; private set; }

    public static MapPos Construct(Cell cell)
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
            var moverAllianceLeader = moveDat.Alliance.Leader.Get(key.Data);
            var cellAllianceLeader = cell.Controller.Get(key.Data).GetAlliance(key.Data).Leader.Get(key.Data);
            throw new Exception($"cell type {cell.GetType().Name}" +
                                $"\nmove type {moveDat.MoveType.Name}" +
                                $"\nlandform {cell.Landform.Get(key.Data).Name}" +
                                $"\nvegetation {cell.Vegetation.Get(key.Data).Name}" +
                                $"\nmover alliance {moverAllianceLeader.Name} {moverAllianceLeader.Id}" +
                                $"\ncell alliance {cellAllianceLeader.Name} {cellAllianceLeader.Id}");
        }
        PolyCell = polyCell;
        Destination = destCell;
        key.Data.Context.AddToMovementRecord(moveDat.Id, this, key.Data);
    }

    public Cell GetCell(Data d)
    {
        return PlanetDomainExt.GetPolyCell(PolyCell, d);
    }
    public MapPos Copy()
    {
        return new MapPos(PolyCell, Destination);
    }
}