using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class ArmiesTab : HBoxContainer, IUiDrawable
{
    private global::MilitaryWindow _parent;
    private VBoxContainer _armyInfo;
    private ItemList _armies;
    public ArmiesTab(global::MilitaryWindow parent)
    {
        _parent = parent;
        _armyInfo = new VBoxContainer();
        _armyInfo.SizeFlagsVertical = SizeFlags.ExpandFill;
        _armyInfo.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_armyInfo);
    }

    private ArmiesTab()
    {
        
    }

    public void Draw(Client c)
    {
        _armies?.QueueFree();
        _armyInfo.ClearChildren();
        var r = _parent.Regime;
        if (r is null) return;
        var armies = c.Data.GetAll<Army>()
            .Where(a => a.Regime.RefId == r.Id);
        
        var armyToken =  new ItemListToken<Army>(
            armies, 
            a => a.Id.ToString(),
            a => DrawArmyInfo(a, c),
            Vector2.One * 20f
        );
        _armies = armyToken.ItemList;
        
        AddChild(_armies);
        _armies.SizeFlagsVertical = SizeFlags.ExpandFill;
        _armies.SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    private void DrawArmyInfo(Army a, Client c)
    {
        _armyInfo.ClearChildren();
        
    }
    
}