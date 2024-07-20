
using System.Linq;

public class ResourceExtractionBudgetBranch : BudgetBranch
{
    public ResourceExtractionBudgetBranch(Regime r, BudgetBranch parent,
        string name, Data d) : base(name)
    {
        Parent = parent;
        foreach (var nr in d.Models.GetModels<NaturalResource>())
        {
            var priority = new ResourceExtractionConstructionPriority(
                nr, r, nr.Name + " extraction");
            Children.Add(new PriorityNode(priority, parent,
                (r, d) =>
                {
                    if (parent.GetRoot().Prices.TryGetValue(nr, out var price))
                    {
                        return price;
                    }

                    return .5f;
                }));
        }
    }

    protected override float GetWeight(Regime r, Data d)
    {
        var prices = GetRoot().Prices;
        var deposits = r.GetCells(d)
            .Where(c => c.HasResourceDeposit(d))
            .Select(c => c.GetResourceDeposit(d))
            .ToArray();
        var emptyCount = deposits.Count(rd => rd.Extraction.IsEmpty());
        return 3f * emptyCount / deposits.Length;
        //todo weight by price, cost
        //
        // var total = deposits.Sum(rd =>
        // {
        //     var nr = (NaturalResource)rd.Item.Get(d);
        //     return prices.TryGetValue(nr, out var price )
        //             ? rd.
        // })
        return .5f;
    }
}