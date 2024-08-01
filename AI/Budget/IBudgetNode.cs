
using System.Linq;

public interface IBudgetNode
{
    string Name { get; }
    float Weight { get; }
    void SetWeights(Regime r, BudgetRoot root, Data d);
}

public static class IBudgetNodeExt
{
    public static float GetTreeWeight(this IBudgetNode n, BudgetRoot root, Data d)
    {
        var mult = 1f;
        var parent = root.GetParent(n);
        var weight = n.Weight;
        while (parent != null)
        {
            if (weight == 0f) return 0f;
            var children = parent.Children;
            var parentChildWeightSum = children.Sum(c => c.Weight);
            var ratio = weight / parentChildWeightSum;
            weight = ratio * parent.Weight;
            parent = root.GetParent(parent);
        }
        
        return weight;
    }
}