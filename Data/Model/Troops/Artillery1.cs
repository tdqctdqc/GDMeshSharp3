using Godot;
using System;
using System.Collections.Generic;

public partial class Artillery1 : Troop
{
    public Artillery1(Items items, FlowList flows) 
        : base(
            nameof(Artillery1),
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
                        {flows.MilitaryCap, 3f}
                    })
            )
        )
    {
    }
}
