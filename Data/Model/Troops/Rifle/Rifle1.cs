
using System.Collections.Generic;

public class Rifle1 : Troop
{
    public Rifle1(Items items) 
        : base(
            nameof(Rifle1),
            TroopDomain.Land,
            new MakeableAttribute(
                IdCount<Item>.Construct(
                    new Dictionary<Item, float>
                    {
                        {items.Recruits, 10}
                    }), 
                1f
            )
        )
    {
    }
}