
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class GoToCellsMission : ArmyMission
{
    public HashSet<int> DestIds { get; private set; }
    public static GoToCellsMission Construct(IEnumerable<Cell> destCells,
        Alliance alliance, Army g, Data d)
    {
        var currWp = g.GetHomeCell(d);
        var moveType = g.MoveType(d);
        
        if (destCells.FirstOrDefault(c => moveType.PassableFriendly(c, alliance, d) == false)
            is Cell c)
        {
            throw new Exception($"{moveType.GetType().Name} cant go to {destCells.GetType().Name}" +
                                $" alliance {alliance.Leader.Get(d).Id}" +
                                $" occupier {c.Controller.RefId} ");
        }
        
        
        return new GoToCellsMission(destCells.Select(c => c.Id).ToHashSet());
    }
    [SerializationConstructor] private GoToCellsMission(
        HashSet<int> destIds)
    {
        DestIds = destIds;
    }
    
    public override void Handle(Army g, LogicWriteKey key, 
        HandleUnitOrdersProcedure proc)
    {
        proc.NewArmyPosesById.TryAdd(g.Id, DestIds);
    }
    public override void Draw(Army group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        
    }

    public override void RegisterCombatActions(Army army, CombatCalculator combat, LogicWriteKey key)
    {
        
    }

    public override string GetDescription(Data d)
    {
        return $"Going to cell {DestIds}";
    }
}