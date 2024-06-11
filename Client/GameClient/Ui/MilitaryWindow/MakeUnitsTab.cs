using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class MakeUnitsTab : HBoxContainer, IUiDrawable
{
    private VBoxContainer _makingUnitsContainer,
        _templatesContainer;

    private ItemListToken<ModelMakeProject> _makingUnits;
    private ItemListToken<UnitTemplate> _templates;
    
    
    private global::MilitaryWindow _parent;
    public MakeUnitsTab(global::MilitaryWindow parent)
    {
        _parent = parent;
        _makingUnitsContainer = new VBoxContainer();
        _makingUnitsContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _makingUnitsContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_makingUnitsContainer);
        
        
        
        _templatesContainer = new VBoxContainer();
        _templatesContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _templatesContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_templatesContainer);
    }
    public void Draw(Client c)
    {
        _makingUnitsContainer.ClearChildren();
        _templatesContainer.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
        _makingUnits?.ItemList.QueueFree();
        var unitProjects = regime.MakeQueue.Queue
            .Where(p => p.Making.Get(c.Data) is UnitTemplate);
        
        // _makingUnits = new ItemListToken<DefaultMakeProject>(
        //     unitProjects,
        //     p => p.Making
        // );


    }
}