using System.Collections.Generic;
using Godot;

public class Barracks : SettlementBuildingModel
{
    public Barracks(Items items, FlowList flows, PeepJobList jobs) 
        : base(
            nameof(Barracks), 
            new List<BuildingModelComponent>
            {
                new BuildingProd(
                    IdCount<IModel>.Construct(
                        (flows.Labor, 100)),

                    IdCount<IModel>.Construct(
                        (items.Recruits, 100),
                        (flows.MilitaryCap, 1000)),
                    
                    IdCount<PeepJob>.Construct(
                        (jobs.Bureaucrat, 100)),
                    flows
                ),
            }, 
            new MakeableAttribute(
                IdCount<IModel>.Construct((flows.ConstructionCap, 10_000)),
                IdCount<IModel>.Construct())
        )
    {
    }

    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}