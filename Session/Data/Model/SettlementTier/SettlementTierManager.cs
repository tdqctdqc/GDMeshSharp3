using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class SettlementTierManager : IModelManager<SettlementTier>
{
    public List<SettlementTier> TiersBySize { get; private set; }
    public Dictionary<string, SettlementTier> Models { get; private set; }
    public static SettlementTier Village { get; private set; } = new SettlementTier(nameof(Village), 0);
    public static SettlementTier Town { get; private set; } = new SettlementTier(nameof(Town), 5);
    public static SettlementTier City { get; private set; } = new SettlementTier(nameof(City), 20);
    // public static SettlementTier Metropolis { get; private set; } = new SettlementTier(nameof(Metropolis), 30);
    
    public SettlementTierManager()
    {
        var settlements = GetType().GetStaticPropertiesOfType<SettlementTier>();
        Models = settlements.ToDictionary(s => s.Name, s => s);
        TiersBySize = settlements.OrderBy(s => s.MinSize).ToList();
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
