
using System.Collections.Generic;
using Godot;

[MessagePack.Union(0, typeof(LineMission))]
public abstract class ArmyMission : IPolymorph
{
    public abstract void Handle(Army g, LogicWriteKey key, HandleUnitMissionsProcedure proc);
    public abstract void RegisterCombatActions(
        Army army, CombatCalculator combat, LogicWriteKey key);
    public abstract bool CleanUp(Army army, ProcedureWriteKey key);
    public abstract string GetDescription(Data d);
    public abstract void Draw(Army group, Vector2 relTo, MeshBuilder mb, Data d);

}