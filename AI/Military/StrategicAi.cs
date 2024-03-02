
using System.Collections.Generic;
using System.Linq;

public class StrategicAi
{
    public Regime Regime { get; private set; }
    private Data _data;
    public HashSet<Theater> Theaters { get; private set; }
    public Dictionary<Cell, float> Targets { get; private set; }

    public StrategicAi(Data data, Regime regime)
    {
        _data = data;
        Regime = regime;
    }
    public void Calculate()
    {
        MakeTheaters();
    }

    private void MakeTheaters()
    {
        var alliance = Regime.GetAlliance(_data);
        var cells = _data.Planet.PolygonAux
            .PolyCells.Cells.Values
            .Where(c => c.Controller.RefId == Regime.Id)
            .ToArray();
        var unions = UnionFind.Find(cells,
            (p, q) => true,
            p => p.GetNeighbors(_data));
        Theaters = new HashSet<Theater>();
        foreach (var union in unions)
        {
            var theaterCells = union.ToHashSet();
            var frontlines = FrontFinder
                .FindFront(theaterCells,
                    p =>
                    {
                        if (p.Controller.IsEmpty()) return false;
                        var pAlliance = p.Controller.Entity(_data).GetAlliance(_data);
                        return alliance.IsRivals(pAlliance, _data);
                    }, _data)
                .Select(fs => new Frontline(fs, Regime))
                .ToHashSet();
            var theater = new Theater(theaterCells, frontlines);
            Theaters.Add(theater);
        }
    }
}