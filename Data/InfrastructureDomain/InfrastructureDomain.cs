using System;
using System.Collections.Generic;
using System.Linq;

public class InfrastructureDomain
{
    public SettlementAux SettlementAux { get; private set; }
    
    public RoadNetwork RoadNetwork => _roads.Value;
    private SingletonCache<RoadNetwork> _roads;
    public InfrastructureDomain() 
    {
    }

    public void Setup(Data data)
    {
        SettlementAux = new SettlementAux(data);
        _roads = new SingletonCache<RoadNetwork>(data);
    }
}
