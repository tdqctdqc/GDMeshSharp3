
using System.Collections.Generic;

public class Rifle1 : Troop
{
    public Rifle1(Items items, TroopTypes troopTypes) 
        : base(
            nameof(Rifle1),
            1, 
            1f, 
            5f, 
            10f, 
            10f,
            new Dictionary<Item, float>
            {
                {items.Recruits, 100}
            }, 
            troopTypes.Infantry,
            new ITroopAttribute[]{})
    {
    }
}