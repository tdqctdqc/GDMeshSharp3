
using System.Collections.Generic;
using System.Linq;

public class StrategicAi
{
    public Alliance Alliance { get; private set; }
    private Data _data;
    public HashSet<Theater> Theaters { get; private set; }

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
        var cells = _data.Planet.MapAux
            .CellHolder.Cells.Values
            .Where(c => alliance.Members.Contains(c.Controller))
            .ToArray();
        var unions = UnionFind.Find(cells,
            (p, q) => true,
            p => p.GetNeighbors(_data));
        Theaters = new HashSet<Theater>();
        foreach (var union in unions)
        {
            var theater = Theater.Construct(Alliance, union.ToHashSet(), _data);
            Theaters.Add(theater);
        }
    }
}