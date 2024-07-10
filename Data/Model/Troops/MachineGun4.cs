using System.Collections.Generic;

public class MachineGun4 : Troop
{
    public MachineGun4(Items items, FlowList flows) 
        : base(
            nameof(MachineGun4),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 2},
                        {flows.IndustrialPower, 10f}
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