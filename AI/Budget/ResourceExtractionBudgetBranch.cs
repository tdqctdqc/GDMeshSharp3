
using System.Collections.Generic;
using System.Linq;

public class ResourceExtractionBudgetBranch : BudgetBranch
{
    public static ResourceExtractionBudgetBranch Construct(Regime r,
        BudgetRoot root,
        Data d)
    {
        var b = new ResourceExtractionBudgetBranch(new List<IBudgetNode>(),
            0f, "Resource Extraction");

        foreach (var nr in d.Models.GetModels<NaturalResource>())
        {
            var priority = new ResourceExtractionConstructionPriority(
                nr.MakeRef<IModel>(), nr.Name + " extraction");
            var node = new PriorityNode(priority);
            root.SetParent(node, b);
        }

        return b;
    }

    public ResourceExtractionBudgetBranch(List<IBudgetNode> children, float weight, string name) : base(children, weight, name)
    {
    }


    protected override float GetWeight(Regime r, BudgetRoot root, Data d)
    {
        var prices = root.Prices;
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