using System.Collections.Generic;

public class MachineGun1 : Troop
{
    public MachineGun1(Items items, FlowList flows) 
        : base(
            nameof(MachineGun1),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 2},
                        {flows.IndustrialPower, 5f}
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