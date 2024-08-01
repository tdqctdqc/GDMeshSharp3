using System;
using System.Collections.Generic;
using Godot;

public partial class BudgetTree : Tree
{
    public event Action<IBudgetNode> SelectedBudgetNode;
    private Dictionary<TreeItem, IBudgetNode> _budgetNodes;
    public BudgetTree(BudgetRoot budgetRoot, Data d)
    {
        _budgetNodes = new Dictionary<TreeItem, IBudgetNode>();
        var root = CreateItem();
        var depth = GetDepth(budgetRoot, 0);
        Columns = depth + 1;
        foreach (var budgetNode in budgetRoot.Children)
        {
            AddBudgetNode(root, budgetRoot, budgetNode, 0, d);
        }

        ItemSelected += () => SelectedBudgetNode.Invoke(_budgetNodes[GetSelected()]);
    }

    private void AddBudgetNode(TreeItem parent, BudgetRoot root, IBudgetNode node,
        int startColumn, Data d)
    {
        var item = CreateItem(parent);
        item.SetCellMode(startColumn, TreeItem.TreeCellMode.String);
        _budgetNodes.Add(item, node);
        var text = $"{node.Name} " +
                   $"Weight: {node.Weight} " +
                   $"Tree Weight: {node.GetTreeWeight(root, d)} ";
        if (node is PriorityNode p)
        {
            text += $"Credit: {p.Credit.GetCredit()}";
        }
        item.SetText(startColumn, text);

        if (node is BudgetBranch b)
        {
            foreach (var budgetNode in b.Children)
            {
                AddBudgetNode(item, root, budgetNode, startColumn + 1, d);
            }
        }
    }

    private int GetDepth(IBudgetNode r, int depth)
    {
        var res = depth + 1;

        if (r is BudgetBranch b)
        {
            foreach (var child in b.Children)
            {
                res = Mathf.Max(res, GetDepth(child, depth));
            }
        }

        return res;
    }
    
}