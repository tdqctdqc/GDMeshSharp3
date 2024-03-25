
public interface IBudgetNode
{
    BudgetBranch Parent { get; }
    ZeroToOne Weight { get; }
}