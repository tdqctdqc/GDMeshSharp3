using System;
using System.Collections.Generic;
using System.Linq;

public class InfrastructureDomain : Domain
{
    public BuildingAux BuildingAux { get; private set; }
    public ConstructionAux ConstructionAux { get; private set; }
    public SettlementAux SettlementAux { get; private set; }
    public CurrentConstruction CurrentConstruction => _construction.Value;
    private SingletonAux<CurrentConstruction> _construction;
    
    public RoadNetwork RoadNetwork => _roads.Value;
    private SingletonAux<RoadNetwork> _roads;
    public InfrastructureDomain(Data data) : base(typeof(InfrastructureDomain), data)
    {
    }

    public override void Setup()
    {
        SettlementAux = new SettlementAux(Data);
        _roads = new SingletonAux<RoadNetwork>(Data);
        BuildingAux = new BuildingAux(Data);
        ConstructionAux = new ConstructionAux(Data);
        _construction = new SingletonAux<CurrentConstruction>(Data);
    }
}
