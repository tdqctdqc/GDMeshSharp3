
using System.Collections.Generic;
using System.Linq;

public class CombatModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var memos = key.Data.HostLogicData.RegimeAis.Dic
            .ToDictionary(kvp => kvp.Key,
                kvp => new MilAiMemo(kvp.Key, key.Data));
        new CombatCalculator().Calculate(key);
        foreach (var milAiMemo in memos.Values)
        {
            milAiMemo.Finish(key);
        }
    }
}