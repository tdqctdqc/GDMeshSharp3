namespace GDMeshSharp3.Data.Model.Troops;


using System.Collections.Generic;

public class Rifle2 : Troop
{
    public Rifle2(Items items, FlowList flows) 
        : base(
            nameof(Rifle2),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 1},
                        {flows.IndustrialPower, 2f}
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