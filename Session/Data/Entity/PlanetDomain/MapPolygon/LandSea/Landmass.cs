using System.Collections.Generic;
using System.Linq;

public class Landmass
{
    public int Id { get; private set; }
    public HashSet<MapPolygon> Polys { get; private set; }

    public Landmass(HashSet<MapPolygon> polys)
    {
        Id = polys.Min(p => p.Id);
        Polys = polys;
    }
}