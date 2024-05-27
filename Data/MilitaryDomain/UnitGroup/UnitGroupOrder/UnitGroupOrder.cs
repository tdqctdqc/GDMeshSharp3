
using System.Collections.Generic;
using Godot;

[MessagePack.Union(0, typeof(LineOrder))]
[MessagePack.Union(1, typeof(DoNothingUnitGroupOrder))]
[MessagePack.Union(2, typeof(GoToCellsOrder))]
public abstract class UnitGroupOrder : IPolymorph
{
    public abstract void Handle(Army g, LogicWriteKey key, HandleUnitOrdersProcedure proc);
    public abstract void Draw(Army group, Vector2 relTo, MeshBuilder mb, Data d);
    public abstract void RegisterCombatActions(
        Army group, CombatCalculator combat, LogicWriteKey key);
    public abstract string GetDescription(Data d);
}