
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UnoccupiedAssignment : ArmyAssignment
{
    public CellRef Cell { get; private set; }

    public UnoccupiedAssignment(int id, DeploymentBranch parent, 
        ERef<Alliance> alliance, HashSet<ERef<Army>> armies, 
        CellRef cell) : base(id, parent, alliance, armies)
    {
        Cell = cell;
    }

    protected override void RemoveGroupFromData(DeploymentAi ai, Army g)
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
        
    }

    public override float Suitability(Army g, Data d)
    {
        return 1f;
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Cell.Get(d);
    }

    public override Army PullGroup(
        Func<Army, float> suitability, 
        LogicKey key)
    {
        if (Armies.Count == 0) return null;
        var g = Armies.MaxBy(v => suitability(v.Get(key.Data)));
        Armies.Remove(g);
        return g.Get(key.Data);
    }
}