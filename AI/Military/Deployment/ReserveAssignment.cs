
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ReserveAssignment : ArmyAssignment
{
    public CellRef Cell { get; private set; }
    
    public ReserveAssignment(int id, DeploymentBranch parent, 
        ERef<Alliance> alliance, HashSet<ERef<Army>> armies, 
        CellRef cell) : base(id, parent, alliance, armies)
    {
        Cell = cell;
    }

    protected override void RemoveArmyFromData(Army g)
    {
        
    }

    public override void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        
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