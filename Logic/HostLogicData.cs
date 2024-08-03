using System;
using System.Collections.Generic;
using System.Linq;

public class HostLogicData
{
    public Cache<Regime, RegimeAi> RegimeAis { get; private set; }
    public Cache<Alliance, AllianceAi> AllianceAis { get; private set; }
    public IdRecycler CombatGraphIds { get; private set; }
    public HostLogicData(Data data)
    {
        //todo make serialized and saved
        RegimeAis = Cache.MakeForEntity<Regime, RegimeAi>
            (r => RegimeAi.Construct(r, data), data);
        AllianceAis = Cache.MakeForEntity<Alliance, AllianceAi>
            (a => AllianceAi.Construct(a, data), data);
        CombatGraphIds = new IdRecycler();
    }
}
