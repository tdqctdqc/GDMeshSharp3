using System;
using System.Collections.Generic;
using System.Linq;

public class HostLogicData
{
    public EntityValueCache<Regime, RegimeAi> RegimeAis { get; private set; }
    public EntityValueCache<Alliance, AllianceAi> AllianceAis { get; private set; }
    public IdRecycler CombatGraphIds { get; private set; }
    public IdRecycler DeploymentTreeIds { get; private set; }
    public HostLogicData(Data data)
    {
        //todo make serialized and saved
        RegimeAis = EntityValueCache<Regime, RegimeAi>
            .ConstructConstant(data, r => new RegimeAi(r, data));
        AllianceAis = EntityValueCache<Alliance, AllianceAi>
            .ConstructConstant(data, a => new AllianceAi(a, data));
        CombatGraphIds = new IdRecycler();
        DeploymentTreeIds = new IdRecycler();
    }
}
