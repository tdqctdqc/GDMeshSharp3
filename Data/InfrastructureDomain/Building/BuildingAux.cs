
using System;

public class BuildingAux
{
    public Indexer<Cell, MapBuilding> ByCell { get; private set; }
    public BuildingAux(Data data)
    {
        ByCell = Indexer.MakeForEntity<Cell, MapBuilding>(
            b => data.Planet.MapAux.CellHolder.Cells[b.Cell.RefId],
            data);
    }
}
