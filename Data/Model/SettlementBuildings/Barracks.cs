using System.Collections.Generic;
using Godot;

public class Barracks : SettlementBuilding
{
    public Barracks(Items items, FlowList flows, PeepJobList jobs) 
        : base(
            nameof(Barracks), 
            new LaborComponent(
                IdCount<Item>.Construct(
                ),

                IdCount<Item>.Construct(
                    (items.Recruits, 100),
                    (flows.MilitaryCap, 1000)),
                    
                IdCount<PeepJob>.Construct(
                    (jobs.Bureaucrat, 100))
            ), 
            new MakeableAttribute(
                IdCount<Item>.Construct((flows.ConstructionCap, 10_000)),
                IdCount<Item>.Construct())
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