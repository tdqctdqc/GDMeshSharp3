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
    
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        var ais = client.Data.HostLogicData.RegimeAis;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        _container.CreateLabelAsChild("BUDGET PRIORITIES");
        foreach (var priority in budget.Priorities)
        {
            var panel = new Panel();
            panel.Size = new Vector2(300f, 500f);
            _container.CreateLabelAsChild($"\t{priority.Name.ToUpper()}");
        }
    }
}