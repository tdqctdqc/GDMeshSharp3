using System.Linq;
using Godot;

public class BudgetPool
{
    public IdCount<IModel> Stock { get; private set; }
    public IdCount<IModel> Net { get; private set; }
    
    public static BudgetPool ConstructForRegime(Regime r, Data d)
    {
        var stock = IdCount<IModel>.Construct();
        stock.Add(r.Stock.Stock);
        
        var net = IdCount<IModel>.ConstructNegative();
        foreach (var (model, value) 
                 in r.Stock.Produced.GetEnumModel(d))
        {
            net.Add(model, value);
        }
        foreach (var (model, value) 
                 in r.Stock.RecurringCosts.GetEnumModel(d))
        {
            net.Remove(model, value);
        }
        foreach (var (model, value) 
                 in r.Stock.SingleTimeCosts.GetEnumModel(d))
        {
            net.Remove(model, value);
        }
        
        var labor = d.Models.Items.Labor.GetNonBuildingSupply(r, d);
        var prods = r.GetProds(d);
        var laborDemand = prods.Sum(p => p.Key.TotalLabor());
        var freeLabor = labor - laborDemand;
        var inQueue = 0f;
        
        r.MakeQueue.Queue.ForEach(m => 
            {
                if (m is ModelMakeProject mp)
                {
                    var making = mp.Model.Get(d);
                    if (making is ResourceExtractionBuilding r)
                    {
                        inQueue += r.BaseLabor();
                        net.Add(r.Resource(d), r.BaseProd());
                    }
                    else if (making is SettlementBuilding b)
                    {
                        inQueue += b.Labor
                            .TotalLabor();
                        foreach (var (model, value) 
                                 in b.Labor.Inputs.GetEnumModel(d))
                        {
                            net.Remove(model, value);
                        }
                        foreach (var (model, value) 
                                 in b.Labor.Outputs.GetEnumModel(d))
                        {
                            net.Add(model, value);
                        }
                    }
                }
            });
        freeLabor -= inQueue;
        freeLabor = Mathf.Max(0f, freeLabor);
        stock.Add(d.Models.Items.Labor, freeLabor);
        return new BudgetPool(stock, net);
    }
    
    
    private BudgetPool(IdCount<IModel> models,
        IdCount<IModel> net)
    {
        Stock = IdCount<IModel>.Construct(models);
        Net = IdCount<IModel>.ConstructNegative(net);
    }
}