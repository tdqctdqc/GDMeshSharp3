using Godot;
namespace Ui.RegimeOverview;

public partial class BudgetTab : ScrollContainer
{
    private VBoxContainer _container;

    public BudgetTab()
    {
        Name = "Budget";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        AddChild(_container);
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
    }
    
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var ais = data.HostLogicData.AIs;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        _container.CreateLabelAsChild("BUDGET PRIORITIES");
        foreach (var priority in budget.Priorities)
        {
            _container.CreateLabelAsChild($"\t{priority.GetType().Name}: {priority.Weight}");
        }
        
        _container.CreateLabelAsChild("ITEM RESERVES");
        foreach (var kvp in budget.Reserve.DesiredReserves)
        {
            var item = kvp.Key;
            var q = kvp.Value;
            var actual = regime.Items[item];
            _container.CreateLabelAsChild($"\t{item.Name}: {actual} / {q}");
        }
    }
}