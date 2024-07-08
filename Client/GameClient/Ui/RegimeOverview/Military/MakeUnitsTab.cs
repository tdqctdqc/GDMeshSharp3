using System;
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
    private Func<Regime> _getRegime;
    
    
    public MakeUnitsTab(Func<Regime> getRegime)
    {
        Name = "Make Units";
        _getRegime = getRegime;

        _makingUnitsInfo = new VBoxContainer();
        _makingUnitsInfo.ExpandFill();
        AddChild(_makingUnitsInfo);
        
        _makingUnitsContainer = new VBoxContainer();
        _makingUnitsContainer.ExpandFill();
        AddChild(_makingUnitsContainer);
        
        _templatesContainer = new VBoxContainer();
        _templatesContainer.ExpandFill();
        AddChild(_templatesContainer);
        
        _templateInfo = new VBoxContainer();
        _templateInfo.ExpandFill();
        AddChild(_templateInfo);
    }
    public void Draw(Client c)
    {
        _makingUnitsContainer.ClearChildren();
        _makingUnitsContainer.CreateLabelAsChild("Units in Progress");
        _makingUnitsInfo.ClearChildren();
        _templatesContainer.ClearChildren();
        _templatesContainer.CreateLabelAsChild("Unit Templates");
        _templateInfo.ClearChildren();
        var regime = _getRegime();
        if (regime is null) return;
        var med = c.Settings.MedIconSize.Value;

        var unitProjects = regime.MakeQueue.Queue
            .OfType<UnitMakeProject>();
        _makingUnits = new ItemListToken<UnitMakeProject>(
            unitProjects,
            p => $"{p.MakingTemplate(c.Data).Name} {p.Fulfilled} / {p.Amount}",
            p => SetMakingUnitsInfo(c),
            p => p.MakingTemplate(c.Data).GetMaxPowerTroop(c.Data).Icon.Texture,
            (int)med);
        _makingUnits.ItemList.ExpandFill();
        _makingUnitsContainer.AddChild(_makingUnits.ItemList);

        var templates = regime.GetUnitTemplates(c.Data);
        _templates = new ItemListToken<UnitTemplate>(
            templates,
            t => t.Name,
            t => SetTemplateInfo(c),
            t => t.GetMaxPowerTroop(c.Data).Icon.Texture,
            (int)med
        );
        _templates.ItemList.ExpandFill();
        _templatesContainer.AddChild(_templates.ItemList);
    }

    private void SetMakingUnitsInfo(Client c)
    {
        var regime = _getRegime();
        _makingUnitsInfo.ClearChildren();
        if (_makingUnits.Value == null) return;
        
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;

        _makingUnitsInfo.AddChild(_makingUnits.Value.GetDisplay(c.Data));
        
        var cancelMakeBtn = _makingUnitsInfo.AddButton(
            "Cancel", () =>
            {
                if (_makingUnits is null || _makingUnits.Value is null)
                {
                    return;
                }

                var proc = new CancelMakeProjectProcedure(regime.MakeRef(),
                    _makingUnits.Value.Id);
                var inner = new SendMessageCommand(proc, player);
                var com = CallbackCommand.Construct(
                    inner, () =>
                    {
                        if (IsInstanceValid(_makingUnits.ItemList))
                        {
                            _makingUnits.Remove(_makingUnits.Value);
                            SetMakingUnitsInfo(c);
                        }
                        
                    }, c);
                c.HandleCommand(com);
            });
    }

    private void SetTemplateInfo(Client c)
    {
        var regime = _getRegime();
        _templateInfo.ClearChildren();
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;
        var template = _templates.Value;
        if (template is null) return;
        _templateInfo.AddChild(template.GetDisplay(c.Data));
        var num = new NumSliderAndEntry("Amount", 1f, 1f, 100f, 1f);
        _templateInfo.AddChild(num);
        var makeBtn = _templateInfo.AddButton(
            "Make", () =>
            {
                if (_templates is null 
                    || _templates.Value is null)
                {
                    return;
                }
                
                var proj = UnitMakeProject.Construct(regime,
                    _templates.Value, (int)num.Value);
                var inner = new StartMakeProjectCommand(proj, player);
                var com = CallbackCommand.Construct(
                    inner, () =>
                    {
                        if (IsInstanceValid(_makingUnits.ItemList))
                        {
                            var newMaking = regime.MakeQueue.Queue
                                .OfType<UnitMakeProject>()
                                .Where(p => _makingUnits.Items.Contains(p) == false)
                                .ToList();
                            foreach (var p in newMaking)
                            {
                                _makingUnits.Add(p);
                            }
                        }
                    }, c);
                c.HandleCommand(com);
            });
    }
}