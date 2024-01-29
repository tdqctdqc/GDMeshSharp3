
using System.Collections.Generic;
using System.Linq;

public class Sea
{
    public int Id { get; private set; }
    public HashSet<MapPolygon> Polys { get; private set; }

    public Sea(HashSet<MapPolygon> polys)
    {
        Id = polys.Min(p => p.Id);
        Polys = polys;
    }
}