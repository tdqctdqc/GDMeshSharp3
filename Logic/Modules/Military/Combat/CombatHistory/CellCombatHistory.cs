
using System.Collections.Generic;
using System.Linq;

public class CellCombatHistory
{
    public int CellId { get; private set; }
    public ERef<Unit>[] AttackerUnits { get; private set; }
    public IdCount<Troop>[] AttackerTroops { get; set; }
    public IdCount<Troop>[] AttackerLosses { get; private set; }
    public IdCount<Troop>[] AttackerKills { get; set; }
    public ERef<Unit>[] DefenderUnits { get; private set; }
    public IdCount<Troop>[] DefenderTroops { get; set; }
    public IdCount<Troop>[] DefenderLosses { get; private set; }
    public IdCount<Troop>[] DefenderKills { get; set; }
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
        
        
        return new CellCombatHistory(
            node.Cell.Id,
            attackers,
            attackerTroops, 
            attackers.Select(d => IdCount<Troop>.Construct()).ToArray(),
            attackers.Select(d => IdCount<Troop>.Construct()).ToArray(),
            defenders, 
            defenderTroops,
            defenders.Select(d => IdCount<Troop>.Construct()).ToArray(),
            defenders.Select(d => IdCount<Troop>.Construct()).ToArray(),
            node.DefendersForcedBack);
    }
    public CellCombatHistory(
        int cellId,
        ERef<Unit>[] attackerUnits, 
        IdCount<Troop>[] attackerTroops, 
        IdCount<Troop>[] attackerLosses, 
        IdCount<Troop>[] attackerKills, 
        ERef<Unit>[] defenderUnits, 
        IdCount<Troop>[] defenderTroops, 
        IdCount<Troop>[] defenderLosses,
        IdCount<Troop>[] defenderKills,
        bool forcedBack)
    {
        CellId = cellId;
        AttackerUnits = attackerUnits;
        AttackerTroops = attackerTroops;
        AttackerLosses = attackerLosses;
        DefenderUnits = defenderUnits;
        DefenderTroops = defenderTroops;
        DefenderLosses = defenderLosses;
        ForcedBack = forcedBack;
    }

    public void AddLossesAndKills(
        Unit attacker, float attackerProportionLosses,
        Unit defender, float defenderProportionLosses)
    {
        var attackerIndex = AttackerUnits.IndexOf(attacker.MakeRef());
        var defenderIndex = DefenderUnits.IndexOf(defender.MakeRef());
        
        foreach (var (troop, num) 
                 in AttackerTroops[attackerIndex].Contents)
        {
            var loss = num * attackerProportionLosses;
            AttackerLosses[attackerIndex].Add(troop, num);
            DefenderKills[defenderIndex].Add(troop, num);
        }
        
        foreach (var (troop, num) 
                 in DefenderTroops[defenderIndex].Contents)
        {
            var loss = num * defenderProportionLosses;
            AttackerKills[attackerIndex].Add(troop, num);
            DefenderLosses[defenderIndex].Add(troop, num);
        }
    }
}