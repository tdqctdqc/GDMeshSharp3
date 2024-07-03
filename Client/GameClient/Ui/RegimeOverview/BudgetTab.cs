using Godot;
namespace Ui.RegimeOverview;

public partial class BudgetTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public BudgetTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Budget";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = ContainerExt.MakeScroll<VBoxContainer>(this);
    }

    private BudgetTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        _container.CreateLabelAsChild("PRIORITIES");
        var small = client.Settings.SmallIconSize.Value;
        var regime = _parent.Regime;
        if (regime is null) return;
        var ais = client.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        var leaves = budget.Root.GetLeaves();
        foreach (var priority in leaves)
        {
            _container.CreateLabelAsChild($"{priority.Priority.Name}");
            _container.CreateLabelAsChild($"    Weight: {priority.GetTreeWeight(client.Data)}");
            _container.CreateLabelAsChild($"    Credit: {priority.Credit.GetCredit().RoundTo2Digits()}");
            if (budget.Root.LastSpending.TryGetValue(priority, out var v))
            {
                _container.CreateLabelAsChild($"    Last spending: {v.spent} on tick {v.tick}");
            }
        }
        _container.AddSpacer(false);

        _container.CreateLabelAsChild("PRICES");
        foreach (var (model, price) in budget.Root.Prices)
        {
            if (model is IIconed i)
            {
                var entry = i.Icon.GetLabeledIcon<HBoxContainer>(
                    $"{model.Name}: {price}", small);
                _container.AddChild(entry);
            }
            else
            {
                _container.CreateLabelAsChild($"{model.Name}: {price}");
            }
        }

    }
}