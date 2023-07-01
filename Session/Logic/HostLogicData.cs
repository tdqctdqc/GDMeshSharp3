using System;
using System.Collections.Generic;
using System.Linq;

public class HostLogicData
{
    public EntityValueCache<Regime, RegimeAi> AIs { get; private set; }

    public HostLogicData(Data data)
    {
        AIs = EntityValueCache<Regime, RegimeAi>
            .ConstructConstant(data, r => new RegimeAi(r, data));
    }
}
