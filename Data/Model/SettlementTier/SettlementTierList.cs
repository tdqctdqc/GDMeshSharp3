using System;
using System.Collections.Generic;
using System.Linq;

public class SettlementTierList : ModelManager<SettlementTier>
{
    public List<SettlementTier> TiersBySize { get; private set; }

    public SettlementTier Village { get; private set; } 
        = new SettlementTier();
    public SettlementTier Town { get; private set; } 
        = new SettlementTier();
    public SettlementTier City { get; private set; } 
        = new SettlementTier();


    public SettlementTierList()
    {
        TiersBySize = Models.OrderBy(s => s.MinSize).ToList();
    }
    public SettlementTier GetTier(int size)
    {
        for (var i = TiersBySize.Count - 1; i >= 0; i--)
        {
            var tier = TiersBySize[i];
            if (size >= tier.MinSize)
            {
                return tier;
            }
        }

        return TiersBySize[0];
        // throw new Exception("Could not find tier for settlement");
    }
}
