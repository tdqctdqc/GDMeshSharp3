
using System.Collections.Generic;
using Godot;

public class DoNothingUnitGroupOrder : UnitGroupOrder
{
    public override void Handle(UnitGroup g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        
    }

    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        return;
    }

    public override CombatResult[] GetCombatResults(UnitGroup g, CombatCalculator.CombatCalcData cData, Data d)
    {
        return this.DefaultCombatResults(g, cData, d);
    }
}