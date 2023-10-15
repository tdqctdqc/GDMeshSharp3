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
        var ais = client.Data.HostLogicData.AIs;
        if (ais.Dic.ContainsKey(regime) == false) return;
        var ai = ais[regime];
        var budget = ai.Budget;
        _container.CreateLabelAsChild("BUDGET PRIORITIES");
        foreach (var priority in budget.Priorities)
        {
            var panel = new Panel();
            panel.Size = new Vector2(300f, 500f);
            _container.CreateLabelAsChild($"\t{priority.Name.ToUpper()}");
        
            _container.CreateLabelAsChild($"\t\tWeight: {priority.Weight}");
            _container.CreateLabelAsChild($"\t\tAccount");
            foreach (var kvp in priority.Account.Items.Contents)
            {
                if (kvp.Value == 0f) continue;
                var item = client.Data.Models.GetModel<Item>(kvp.Key);
                _container.CreateLabelAsChild($"\t\t{item.Name}: {kvp.Value.RoundTo2Digits()}");
            }
            
            var wishlist = priority.Wishlist;
            if (wishlist != null)
            {
                _container.CreateLabelAsChild($"\t\tWishlist");
                foreach (var kvp in wishlist)
                {
                    if (kvp.Value == 0f) continue;
                    _container.CreateLabelAsChild($"\t\t{kvp.Key.Name}: {kvp.Value}");
                }
            }
        }
    }
}