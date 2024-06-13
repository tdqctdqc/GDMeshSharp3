using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux
{
    public ERefColIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public AllianceAux(Data data)
    {
        RegimeAlliances = new ERefColIndexer<Alliance, Regime>(
            a => a.Members.Entities(data), data);
        data.SubscribeForDestruction<Alliance>(n => data.Notices.Political.AllianceDissolved.Invoke((Alliance)n.Entity));
    }
}
