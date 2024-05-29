
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Theater
{
    public HashSet<Cell> Cells { get; private set; }
    public HashSet<Frontline> Frontlines { get; private set; }

    public static Theater Construct(
        Alliance alliance,
        HashSet<Cell> theaterCells,
        Data data)
    {
        var frontlines = FrontFinder
            .FindFront(theaterCells,
                p =>
                {
                    if (p.Controller.IsEmpty()) return false;
                    var pAlliance = p.Controller.Get(data).GetAlliance(data);
                    return alliance.IsRivals(pAlliance, data);
                }, data)
            .Select(fs => new Frontline(fs, 
                new HashSet<Cell>(), 
                alliance))
            .ToHashSet();
        
        return new Theater(theaterCells, frontlines);
    }
    
    [SerializationConstructor] private Theater(HashSet<Cell> cells, HashSet<Frontline> frontlines)
    {
        Cells = cells;
        Frontlines = frontlines;
    }
}