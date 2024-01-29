using System;
using System.Collections.Generic;
using System.Linq;

public class InfrastructureDomain
{
    public BuildingAux BuildingAux { get; private set; }
    public ConstructionAux ConstructionAux { get; private set; }
    public SettlementAux SettlementAux { get; private set; }
    public CurrentConstruction CurrentConstruction => _construction.Value;
    private SingletonAux<CurrentConstruction> _construction;
    
    public RoadNetwork RoadNetwork => _roads.Value;
    private SingletonAux<RoadNetwork> _roads;
    public InfrastructureDomain() 
    {
    }

    public void Setup(Data data)
    {
        SettlementAux = new SettlementAux(data);
        _roads = new SingletonAux<RoadNetwork>(data);
        BuildingAux = new BuildingAux(data);
        ConstructionAux = new ConstructionAux(data);
        _construction = new SingletonAux<CurrentConstruction>(data);
    }
}
