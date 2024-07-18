using System;
using System.Collections.Generic;
using System.Linq;

public class SettlementTierList : ModelManager<SettlementTier>
{
    public SettlementTier Village { get; private set; } 
        = new SettlementTier();
    public SettlementTier Town { get; private set; } 
        = new SettlementTier();
    public SettlementTier City { get; private set; } 
        = new SettlementTier();
    public SettlementTierList()
    {
    }
}
