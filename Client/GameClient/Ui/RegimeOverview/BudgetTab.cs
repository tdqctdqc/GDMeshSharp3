using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class BudgetTab : ScrollContainer, IUiDrawable
{
    private Container _container, _priorityInfo;
    private RegimeOverviewWindow _parent;
    public BudgetTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Budget";
        _container = new HBoxContainer();
        AddChild(_container);
        _container.FullRect();
    }

    private BudgetTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        _container.ExpandFill();
        var small = client.Settings.SmallIconSize.Value;
        var regime = _parent.Regime;
        if (regime is null) return;
        var ais = client.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        
        var left = _container.MakeScrollChild<VBoxContainer>(
            out var leftScroll);
        leftScroll.ExpandFill(1);
        left.ExpandFill();

        _priorityInfo = _container.MakeScrollChild<VBoxContainer>(
            out var priorityScroll);
        priorityScroll.ExpandFill(1);
        _priorityInfo.ExpandFill();
        
        var budgetTree = new BudgetTree(budget.Root, client.Data);
        budgetTree.SelectedBudgetNode += n =>
        {
            if (n is PriorityNode p)
            {
                DrawPriorityInfo(budget.Root, p, client);
            }
        };
        budgetTree.ExpandFill();
        left.AddChild(budgetTree);
        
        left.CreateLabelAsChild("Prices");
        var priceContainer = left.MakeScrollChild<VBoxContainer>(
            out var priceScroll);
        priceScroll.ExpandFill();
        
        foreach (var (model, price) in budget.Root.RelativePrices(client.Data))
        {
            if (model is IIconed i)
            {
                var entry = i.Icon.GetLabeledIcon<HBoxContainer>(
                    $"{model.Name}: {price}", small);
                priceContainer.AddChild(entry);
            }
            else
            {
                priceContainer.CreateLabelAsChild($"{model.Name}: {price}");
            }
        }
    }

    private void DrawPriorityInfo(BudgetRoot root, PriorityNode node, Client c)
    {
        _priorityInfo.ClearChildren();
        var priority = node.Priority;
        if (priority == null) return;
        var regime = _parent.Regime;
        var ais = c.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        
        var small = c.Settings.SmallIconSize.Value;
        var med = c.Settings.MedIconSize.Value;
        
        
        _priorityInfo.CreateLabelAsChild($"Credit: {node.Credit.GetCredit()}");
        _priorityInfo.CreateLabelAsChild($"Weight: {node.GetTreeWeight(budget.Root, c.Data)}");
        var made = node.MadeByTick;

        var madeBox = _priorityInfo.MakeScrollChild<VBoxContainer>(
            out var madeScroll);
        madeScroll.ExpandFill();
        
        foreach (var (tick, tickMade) in made.OrderByDescending(kvp => kvp.Key))
        {
            madeBox.CreateLabelAsChild("Tick " + tick);
            foreach (var (name, amtMade) in tickMade)
            {
                madeBox.CreateLabelAsChild($"{name}: {amtMade}");
            }
        }
        
        
        _priorityInfo.CreateLabelAsChild($"Wishlist");

        
        
    }
}