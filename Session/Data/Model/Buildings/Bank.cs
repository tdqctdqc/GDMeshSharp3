using System;
using System.Collections.Generic;
using System.Linq;

public class Bank : FinancialBuildingModel
{
    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
        = new Dictionary<PeepJob, int>
        {
            {PeepJobManager.Bureaucrat, 500}
        };
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.FinancialPower, 10_000}
        };
    public Bank() 
        : base(500, nameof(Bank), 25, 200)
    {
    }
}
