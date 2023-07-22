using System;
using System.Collections.Generic;
using System.Linq;

public class InfrastructureDomain : Domain
{
    
    public RoadAux RoadAux { get; private set; }
    public EntityRegister<RoadSegment> RoadSegments => Data.GetRegister<RoadSegment>();
    public EntityRegister<Settlement> Settlements => Data.GetRegister<Settlement>();
    public BuildingAux BuildingAux { get; private set; }

    public SettlementAux SettlementAux { get; private set; }

    public EntityRegister<MapBuilding> Buildings => Data.GetRegister<MapBuilding>();
    public CurrentConstruction CurrentConstruction => _construction.Value;
    private SingletonAux<CurrentConstruction> _construction;
    public InfrastructureDomain(Data data) : base(typeof(InfrastructureDomain), data)
    {
    }

    public override void Setup()
    {
        SettlementAux = new SettlementAux(Data);
        RoadAux = new RoadAux(Data);
        BuildingAux = new BuildingAux(Data);
        _construction = new SingletonAux<CurrentConstruction>(Data);

    }
}