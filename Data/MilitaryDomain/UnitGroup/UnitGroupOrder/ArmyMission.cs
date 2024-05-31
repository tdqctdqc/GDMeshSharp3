
using System.Collections.Generic;
using Godot;

[MessagePack.Union(0, typeof(LineMission))]
[MessagePack.Union(1, typeof(DoNothingArmyMission))]
[MessagePack.Union(2, typeof(GoToCellsMission))]
public abstract class ArmyMission : IPolymorph
{
    public abstract void Handle(Army g, LogicWriteKey key, HandleUnitOrdersProcedure proc);
    public abstract void Draw(Army group, Vector2 relTo, MeshBuilder mb, Data d);
    public abstract void RegisterCombatActions(
        Army army, CombatCalculator combat, LogicWriteKey key);
    public abstract string GetDescription(Data d);
}