
using System.Collections.Generic;
using System.Linq;

public class CellCombatHistory
{
    public ERef<Unit>[] AttackerUnits { get; private set; }
    public IdCount<Troop>[] AttackerTroops { get; set; }
    public IdCount<Troop>[] AttackerLosses { get; private set; }
    
    public ERef<Unit>[] DefenderUnits { get; private set; }
    public IdCount<Troop>[] DefenderTroops { get; set; }
    public IdCount<Troop>[] DefenderLosses { get; private set; }
    public bool ForcedBack { get; private set; }
    public static CellCombatHistory Construct(CellCombatNode node)
    {
        var attackers = node.Attackers
            .Select(m => m.Unit.MakeRef()).ToArray();
        var defenders = node.Defenders
            .Select(m => m.Unit.MakeRef()).ToArray();
        var attackerTroops = node.Attackers
            .Select(m =>
            {
                var troops = m.Unit.Troops;
                var proportion = m.Proportion;
                return new IdCount<Troop>(
                    troops.Contents
                        .ToDictionary(
                            kvp => kvp.Key, 
                            kvp => kvp.Value * proportion),
                    false);
            }).ToArray();
        var defenderTroops = node.Defenders
            .Select(m =>
            {
                var troops = m.Unit.Troops;
                var proportion = m.Proportion;
                return new IdCount<Troop>(
                    troops.Contents
                        .ToDictionary(
                            kvp => kvp.Key, 
                            kvp => kvp.Value * proportion),
                    false);
            }).ToArray();
        
        var attackerLosses = node.Attackers
            .Select(m =>
            {
                var troops = m.Unit.Troops;
                var proportion = m.Proportion;
                var proportionLosses = m.ProportionLosses;
                return new IdCount<Troop>(
                    troops.Contents
                        .ToDictionary(
                            kvp => kvp.Key, 
                            kvp => kvp.Value * proportion * proportionLosses),
                    false);
            }).ToArray();
        var defenderLosses = node.Defenders
            .Select(m =>
            {
                var troops = m.Unit.Troops;
                var proportion = m.Proportion;
                var proportionLosses = m.ProportionLosses;
                return new IdCount<Troop>(
                    troops.Contents
                        .ToDictionary(
                            kvp => kvp.Key, 
                            kvp => kvp.Value * proportion * proportionLosses),
                    false);
            }).ToArray();
        return new CellCombatHistory(attackers,
            attackerTroops, attackerLosses,
            defenders, defenderTroops, defenderLosses,
            node.DefendersForcedBack);
    }
    public CellCombatHistory(ERef<Unit>[] attackerUnits, 
        IdCount<Troop>[] attackerTroops, 
        IdCount<Troop>[] attackerLosses, 
        ERef<Unit>[] defenderUnits, 
        IdCount<Troop>[] defenderTroops, 
        IdCount<Troop>[] defenderLosses,
        bool forcedBack)
    {
        AttackerUnits = attackerUnits;
        AttackerTroops = attackerTroops;
        AttackerLosses = attackerLosses;
        DefenderUnits = defenderUnits;
        DefenderTroops = defenderTroops;
        DefenderLosses = defenderLosses;
        ForcedBack = forcedBack;
    }
}