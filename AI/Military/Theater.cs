
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Theater : Entity 
{
    public HashSet<CellRef> Cells { get; private set; }
    public ERefSet<Frontline> Frontlines { get; private set; }
    
    public static Theater Create(
        Alliance alliance,
        HashSet<Cell> theaterCells,
        ICreateKey key)
    {
        var data = key.GetData();
        var frontlines = FrontFinder
            .FindFrontsLeftToRight(theaterCells,
                p =>
                {
                    if (p.Controller.IsEmpty()) return false;
                    var pAlliance = p.Controller.Get(data).GetAlliance(data);
                    return alliance.IsRivals(pAlliance, data);
                }, data)
            .Select(fs => Frontline.Create(fs, 
                new HashSet<CellRef>(), 
                alliance, key))
            .ToArray()
            .Select(fl => fl.MakeRef());
        
        var t = new Theater(data.IdDispenser.TakeId(),
            theaterCells.Select(c => c.MakeRef()).ToHashSet(), 
            ERefSet<Frontline>.Construct(frontlines) );
        key.Create(t);
        return t;
    }
    
    [SerializationConstructor] private 
        Theater(int id,
            HashSet<CellRef> cells, ERefSet<Frontline> frontlines)
        : base(id)
    {
        Id = id;
        Cells = cells;
        Frontlines = frontlines;
    }

    public override void CleanUp(IWriteKey key)
    {
        
    }
}