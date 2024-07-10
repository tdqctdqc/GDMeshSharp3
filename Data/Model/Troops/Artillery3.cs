using Godot;
using System;
using System.Collections.Generic;

public class Artillery3 : Troop
{
    public Artillery3(Items items, FlowList flows) 
        : base(
            nameof(Artillery3),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        {items.Recruits, 2},
                        {flows.IndustrialPower, 25f}
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