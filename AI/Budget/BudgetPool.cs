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
        
        var pop = r.GetCells(d).Sum(c => c.GetPeep(d).Size);
        var prods = r.GetProds(d);
        var laborDemand = prods.Sum(p => p.Key.TotalLabor());
        var freeLabor = pop - laborDemand;
        var inQueue = 0f;
        
        r.MakeQueue.Queue.ForEach(m => 
            {
                var making = m.Making.Get(d);
                if (making is ResourceExtractionBuilding r)
                {
                    inQueue += r.BaseLabor;
                    net.Add(r.Resource, r.BaseProd);
                }
                else if (making is SettlementBuildingModel b
                         && b.GetComponent<LaborComponent>() 
                             is LaborComponent l)
                {
                    inQueue += l
                        .TotalLabor();
                    foreach (var (model, value) 
                        in l.Inputs.GetEnumModel(d))
                    {
                        net.Remove(model, value);
                    }
                    foreach (var (model, value) 
                             in l.Outputs.GetEnumModel(d))
                    {
                        net.Add(model, value);
                    }
                }
            });
        freeLabor -= inQueue;
        freeLabor = Mathf.Max(0f, freeLabor);
        stock.Add(d.Models.Flows.Labor, freeLabor);
        return new BudgetPool(stock, net);
    }
    
    
    private BudgetPool(IdCount<IModel> models,
        IdCount<IModel> net)
    {
        Stock = IdCount<IModel>.Construct(models);
        Net = IdCount<IModel>.ConstructNegative(net);
    }
}