
using System.Collections.Generic;
using Godot;

[MessagePack.Union(0, typeof(DeployOnLineGroupOrder))]
[MessagePack.Union(1, typeof(DoNothingUnitGroupOrder))]
[MessagePack.Union(2, typeof(GoToCellGroupOrder))]
public abstract class UnitGroupOrder : IPolymorph
{
    public abstract void Handle(UnitGroup g, LogicWriteKey key, HandleUnitOrdersProcedure proc);
    public abstract void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d);
    public abstract void RegisterCombatActions(
        UnitGroup group, CombatCalculator combat, LogicWriteKey key);
    public abstract string GetDescription(Data d);
}