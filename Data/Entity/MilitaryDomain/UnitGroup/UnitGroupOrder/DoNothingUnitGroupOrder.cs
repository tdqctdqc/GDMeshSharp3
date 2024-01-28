
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

    public override void RegisterCombatActions(CombatCalculator combat, LogicWriteKey key)
    {
        
    }
}