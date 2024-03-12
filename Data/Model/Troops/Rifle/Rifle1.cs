
using System.Collections.Generic;

public class Rifle1 : Troop
{
    public Rifle1(Items items, FlowList flows) 
        : base(
            nameof(Rifle1),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 1},
                        {flows.IndustrialPower, 1f}
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