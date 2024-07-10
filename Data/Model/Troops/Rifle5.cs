using System.Collections.Generic;

public class Rifle5 : Troop
{
    public Rifle5(Items items, FlowList flows) 
        : base(
            nameof(Rifle5),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 1},
                        {flows.IndustrialPower, 8f}
                    }),
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {flows.MilitaryCap, 1f}
                    })
            )
        )
    {
    }
}