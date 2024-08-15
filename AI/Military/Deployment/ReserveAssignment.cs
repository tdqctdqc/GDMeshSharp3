
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ReserveAssignment : ArmyAssignment
{
    public ERef<Theater> Theater { get; private set; }
    public CellRef Cell { get; private set; }
    
    public ReserveAssignment(DeploymentBranch parent, 
        ERef<Alliance> alliance, HashSet<ERef<Army>> armies, 
        CellRef cell, ERef<Theater> theater, int id) 
            : base(parent, alliance, armies, id)
    {
        Cell = cell;
        Theater = theater;
    }

    protected override void RemoveArmyFromData(Army g)
    {
        
    }

    public override void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        
    }

    public override void MergeToNew(DeploymentRoot newRoot, StrategicContext context, LogicKey key)
    {
        var aRef = Armies.Single();
        if (key.Data.HasEntity(aRef.RefId) == false) return;
        var army = aRef.Get(key.Data);
        var home = army.GetHomeCell(key.Data);
        
        var merges = context.TheaterMerges[Theater];
        var newTheaters = newRoot
            .GetDescendentNodesOfType<TheaterBranch>()
            .ToDictionary(v => v.Theater.Get(key.Data),
                v => v);
        
        var mergeTheater = newTheaters.Keys
            .First(t => t.Cells.Contains(home.MakeRef()));
        
        var mergeReserve = newTheaters[mergeTheater]
            .GetDescendentAssignmentsOfType<ReserveAssignment>()
            .Single();
        if (mergeReserve.Armies.Any())
        {
            var mergeArmy = mergeReserve.Armies.Single().Get(key.Data);
            
        }
        else
        {
            mergeReserve.PushArmy(army, key);
        }
    }

    protected override void AddGroupToData(Army g, Data d)
    {
        
    }
    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    

    public override void SetWeights(LogicKey key)
    {
        
    }

    public override void GiveOrders(LogicKey key)
    {
        var army = Armies.Single().Get(key.Data);
        var order = new LineMission(
            new RefSet<CellRef>(
                Cell.Yield().ToHashSet()),
            new RefSet<CellRef>(new HashSet<CellRef>()),
            false);
        var proc = new SetUnitOrderProcedure(
            army.MakeRef(), order);
        key.SendMessage(proc);
    }

    public override float Suitability(Unit g, Data d)
    {
        return 1f;
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Cell.Get(d);
    }

    public override Unit PullUnit(
        Func<Unit, float> suitability, 
        LogicKey key)
    {
        if (Armies.Count == 0) return null;
        var g = Armies
            .SelectMany(a => a.Get(key.Data).Units.Entities(key.Data))
            .MaxBy(v => suitability(v));
        return g;
    }
}