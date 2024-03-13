using System.Collections.Generic;
using Godot;

public class Barracks : BuildingModel
{
    public Barracks(Items items, FlowList flows, PeepJobList jobs) 
        : base(BuildingType.Military, 
            nameof(Barracks), 10,
            500, 
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
                IdCount<IModel>.Construct(),
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