
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
        LogicKey key)
    {
        var cell = PlanetDomainExt.GetPolyCell(polyCell, key.Data);
        if (moveDat.MoveType.PassableFriendly(cell, moveDat.Regime, key.Data) == false)
        {
            var moverRegime = moveDat.Regime;
            var cellRegime = cell.Controller.Get(key.Data);
            throw new Exception($"cell type {cell.GetType().Name}" +
                                $"\nmove type {moveDat.MoveType.Name}" +
                                $"\nlandform {cell.Landform.Get(key.Data).Name}" +
                                $"\nvegetation {cell.Vegetation.Get(key.Data).Name}" +
                                $"\nmover alliance {moverRegime.Name} {moverRegime.Id}" +
                                $"\ncell alliance {cellRegime.Name} {cellRegime.Id}");
        }
        PolyCell = polyCell;
        Destination = destCell;
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