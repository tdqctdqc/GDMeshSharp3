
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Theater : Entity 
{
    public ERef<Alliance> Alliance { get; private set; }
    public HashSet<CellRef> Cells { get; private set; }
    public ERefSet<Frontline> Frontlines { get; private set; }
    
    public static Theater Create(
        Alliance alliance,
        HashSet<Cell> theaterCells,
        ICreateKey key)
    {
        var data = key.GetData();
        var t = new Theater(data.IdDispenser.TakeId(),
            theaterCells.Select(c => c.MakeRef()).ToHashSet(), 
            ERefSet<Frontline>.Construct(new HashSet<int>()), 
            alliance.MakeRef());
        key.Create(t);
        return t;
    }
    public static Theater Create(
        Alliance alliance,
        HashSet<CellRef> theaterCells,
        ICreateKey key)
    {
        var data = key.GetData();
        var t = new Theater(data.IdDispenser.TakeId(),
            theaterCells.ToHashSet(), 
            ERefSet<Frontline>.Construct(new HashSet<int>()), 
            alliance.MakeRef());
        key.Create(t);
        return t;
    }
    
    [SerializationConstructor] private 
        Theater(int id,
            HashSet<CellRef> cells, ERefSet<Frontline> frontlines,
            ERef<Alliance> alliance)
        : base(id)
    {
        Alliance = alliance;
        Id = id;
        Cells = cells;
        Frontlines = frontlines;
    }

    public void MakeFrontlinesFromScratch(LogicKey key)
    {
        var data = key.Data;
        var alliance = Alliance.Get(key.Data);
        
        var frontlines = 
            Frontline.GetFacesFromCells(Cells.Select(c => c.Get(key.Data)),
            alliance, data)
                .Select(l => Frontline.Create(l, new HashSet<CellRef>(),
                    alliance, key))
                .Select(f => f.MakeRef()).ToArray();
            
        Frontlines = ERefSet<Frontline>.Construct(frontlines);
    }
    public void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        foreach (var cRef in Cells)
        {
            var cell = cRef.Get(d);
            mb.DrawPolygonRel(cell.AbsBoundary(d).ToArray(),
                Colors.Blue.Tint(.5f), relTo, d);
        }
    }
    public override void CleanUp(ProcedureKey key)
    {
        
    }
}