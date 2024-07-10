using System.Collections.Generic;

public class MachineGun3 : Troop
{
    public MachineGun3(Items items, FlowList flows) 
        : base(
            nameof(MachineGun3),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 2},
                        {flows.IndustrialPower, 8.5f}
                    }),
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {flows.MilitaryCap, 2f}
                    })
            )
        )
    {
    }
}