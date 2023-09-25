using System.Collections.Generic;

public class Infantry : TroopType
{
    public Infantry() 
        : base(nameof(Infantry), TroopDomain.Land)
    { }
}