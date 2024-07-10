using Godot;
using System;
using System.Collections.Generic;

public class Artillery2 : Troop
{
    public Artillery2(Items items, FlowList flows) 
        : base(
            nameof(Artillery2),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 2},
                        {flows.IndustrialPower, 15f}
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