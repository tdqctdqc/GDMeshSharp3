
using System.Collections.Generic;
using System.Linq;

public class StrategicAi
{
    public Alliance Alliance { get; private set; }
    private Data _data;
    public HashSet<Theater> Theaters { get; private set; }
    public Dictionary<Cell, float> Targets { get; private set; }

    public StrategicAi(Data data, Alliance alliance)
    {
        _data = data;
        Alliance = alliance;
        
    }
    public void Calculate()
    {
        MakeTheaters();
    }

    private void MakeTheaters()
    {
        var alliance = Alliance;
        var cells = _data.Planet.PolygonAux
            .PolyCells.Cells.Values
            .Where(c => alliance.Members.RefIds.Contains(c.Controller.RefId))
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
                        var pAlliance = p.Controller.Get(_data).GetAlliance(_data);
                        return alliance.IsRivals(pAlliance, _data);
                    }, _data)
                .Select(fs => new Frontline(fs, Alliance))
                .ToHashSet();
            var theater = new Theater(theaterCells, frontlines);
            Theaters.Add(theater);
        }
    }
}