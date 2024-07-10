using System.Collections.Generic;

public class MachineGun2 : Troop
{
    public MachineGun2(Items items, FlowList flows) 
        : base(
            nameof(MachineGun2),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 2},
                        {flows.IndustrialPower, 7.5f}
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