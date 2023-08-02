using System;
using System.Collections.Generic;
using System.Linq;

public class SettlementTierList : ModelList<SettlementTier>
{
    public List<SettlementTier> TiersBySize { get; private set; }

    public SettlementTier Village { get; private set; } = new SettlementTier(nameof(Village), 0);
    public SettlementTier Town { get; private set; } = new SettlementTier(nameof(Town), 5);
    public SettlementTier City { get; private set; } = new SettlementTier(nameof(City), 20);


    public SettlementTierList()
    {
        var models = this.GetPropertiesOfType<SettlementTier>();
        TiersBySize = models.OrderBy(s => s.MinSize).ToList();
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
        throw new Exception("Could not find tier for settlement");
    }
}
