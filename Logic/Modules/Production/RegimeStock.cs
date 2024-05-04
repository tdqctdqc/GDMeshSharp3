
using System.Collections.Generic;
using MessagePack;

public class RegimeStock
{
    public IdCount<IModel> Stock { get; private set; }
    public IdCount<IModel> SingleTimeCosts { get; private set; }
    public IdCount<IModel> RecurringCosts { get; private set; }
    public IdCount<IModel> Produced { get; private set; }
    public Dictionary<int, PeepEmploymentReport> EmploymentReports { get; private set; }

    public static RegimeStock Construct()
    {
        return new RegimeStock(
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct(),
            new Dictionary<int, PeepEmploymentReport>()            
        );
    }
    [SerializationConstructor] private RegimeStock(
        IdCount<IModel> stock, 
        IdCount<IModel> singleTimeCosts, 
        IdCount<IModel> recurringCosts, 
        IdCount<IModel> produced, 
        Dictionary<int, PeepEmploymentReport> employmentReports)
    {
        Stock = stock;
        SingleTimeCosts = singleTimeCosts;
        RecurringCosts = recurringCosts;
        Produced = produced;
        EmploymentReports = employmentReports;
    }
}