

using System.Collections.Generic;

public class Rifle4 : Troop
{
    public Rifle4(Items items, FlowList flows) 
        : base(
            nameof(Rifle4),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 1},
                        {flows.IndustrialPower, 5f}
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