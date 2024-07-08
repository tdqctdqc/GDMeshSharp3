
using System.Linq;

public interface IBudgetNode
{
    string Name { get; }
    BudgetBranch Parent { get; }
    float Weight { get; }
    void SetWeights(Regime r, Data d);
}

public static class IBudgetNodeExt
{
    public static float GetTreeWeight(this IBudgetNode n, Data d)
    {
        var mult = 1f;
        var parent = n.Parent;
        var weight = n.Weight;
        while (parent != null)
        {
            var children = parent.Children;
            var parentChildWeightSum = children.Sum(c => c.Weight);
            var ratio = weight / parentChildWeightSum;
            weight = ratio * parent.Weight;
            parent = parent.Parent;
        }
        
        return weight;
    }
}