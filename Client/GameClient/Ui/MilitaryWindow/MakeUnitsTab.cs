using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class MakeUnitsTab : HBoxContainer, IUiDrawable
{
    private VBoxContainer _makingUnitsContainer,
        _makingUnitsInfo,
        _templateInfo,
        _templatesContainer;

    private ItemListToken<UnitMakeProject> _makingUnits;
    private ItemListToken<UnitTemplate> _templates;
    
    
    private global::MilitaryWindow _parent;
    public MakeUnitsTab(global::MilitaryWindow parent)
    {
        _parent = parent;

        _makingUnitsInfo = new VBoxContainer();
        _makingUnitsInfo.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _makingUnitsInfo.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_makingUnitsInfo);
        
        
        
        _makingUnitsContainer = new VBoxContainer();
        _makingUnitsContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _makingUnitsContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_makingUnitsContainer);
        
        
        
        _templatesContainer = new VBoxContainer();
        _templatesContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _templatesContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_templatesContainer);
        
        _templateInfo = new VBoxContainer();
        _templateInfo.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _templateInfo.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_templateInfo);
    }
    public void Draw(Client c)
    {
        _makingUnitsContainer.ClearChildren();
        _makingUnitsInfo.ClearChildren();
        _templatesContainer.ClearChildren();
        _templateInfo.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;

        
        var unitProjects = regime.MakeQueue.Queue
            .OfType<UnitMakeProject>();
        _makingUnits = new ItemListToken<UnitMakeProject>(
            unitProjects,
            p => p.MakingTemplate(c.Data).Name,
            p => SetMakingUnitsInfo(c),
            Vector2.One * 40f,
            p => p.MakingTemplate(c.Data).GetMaxPowerTroop(c.Data).Icon.Texture,
            Vector2I.One * 40);
        _makingUnits.ItemList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _makingUnits.ItemList.SizeFlagsVertical = SizeFlags.ExpandFill;
        _makingUnitsContainer.AddChild(_makingUnits.ItemList);



        var templates = regime.GetUnitTemplates(c.Data);
        _templates = new ItemListToken<UnitTemplate>(
            templates,
            t => t.Name,
            t => SetTemplateInfo(c),
            Vector2.One * 40f,
            t => t.GetMaxPowerTroop(c.Data).Icon.Texture,
            Vector2I.One * 40
        );
        _templates.ItemList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _templates.ItemList.SizeFlagsVertical = SizeFlags.ExpandFill;
        _templatesContainer.AddChild(_templates.ItemList);
    }

    private void SetMakingUnitsInfo(Client c)
    {
        var regime = _parent.Regime;
        _makingUnitsInfo.ClearChildren();
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;

        var cancelMakeBtn = _makingUnitsInfo.AddButton(
            "Cancel", () =>
            {
                if (_makingUnits is null || _makingUnits.Selected is null)
                {
                    return;
                }
                var com = new CancelMakeProjectCommand(regime.MakeRef(),
                    _makingUnits.Selected.Id, player);
                c.HandleCommand(com);
                _makingUnits.Remove(_makingUnits.Selected);
            });
    }

    private void SetTemplateInfo(Client c)
    {
        var regime = _parent.Regime;
        _templateInfo.ClearChildren();
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;

        var makeBtn = _templateInfo.AddButton(
            "Make", () =>
            {
                if (_templates is null 
                    || _templates.Selected is null)
                {
                    return;
                }

                var proj = UnitMakeProject.Construct(regime,
                    _templates.Selected);
                var com = new StartMakeProjectCommand(proj, player);
                c.HandleCommand(com);
                // _makingUnits.Remove(_makingUnits.Selected);
            });
    }
}