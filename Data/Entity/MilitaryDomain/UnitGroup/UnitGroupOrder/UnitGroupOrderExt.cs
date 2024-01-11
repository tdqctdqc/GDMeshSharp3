
using System.Collections.Generic;
using System.Linq;

public static class UnitGroupOrderExt
{
    public static CombatResult[]
        InitializeResultsWithLosses(this UnitGroupOrder order,
            UnitGroup group, CombatCalculator.CombatCalcData cData, Data d)
    {
        return group.Units.Items(d)
            .Select(u => CombatResult.Construct(u, cData, d))
            .Where(r => r != null)
            .ToArray();
    }
    
    public static CombatResult[] DefaultCombatResults(this UnitGroupOrder order,
        UnitGroup group, CombatCalculator.CombatCalcData cData, Data d)
    {
        var results = group.Units.Items(d)
            .Select(u => CombatResult.Construct(u, cData, d))
            .Where(r => r != null)
            .ToArray();
        
        
        
        return results;
    }
}