
using System.Collections.Generic;

public class Rifle3 : Troop
{
    public Rifle3(Items items, FlowList flows) 
        : base(
            nameof(Rifle3),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 1},
                        {flows.IndustrialPower, 3f}
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