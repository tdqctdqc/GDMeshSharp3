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
    public void Draw(Client client)
    {
        _makingUnitsContainer.ClearChildren();
        _makingUnitsContainer.CreateLabelAsChild("Units in Progress");
        _makingUnitsInfo.ClearChildren();
        _templatesContainer.ClearChildren();
        _templatesContainer.CreateLabelAsChild("Unit Templates");
        _templateInfo.ClearChildren();
        var regime = _getRegime();
        if (regime is null) return;
        var med = client.Settings.MedIconSize.Value;

        var unitProjects = regime.MakeQueue.Queue
            .OfType<UnitMakeProject>();
        _makingUnits = new ItemListToken<UnitMakeProject>(
            unitProjects,
            p => $"{p.MakingTemplate(client.Data).Name} {p.Fulfilled} / {p.Amount}",
            p => p.MakingTemplate(client.Data).GetMaxPowerTroop(client.Data).Icon.Texture,
            (int)med,
            true);
        _makingUnits.JustSelected += () => SetMakingUnitsInfo(client);
        _makingUnits.ItemList.ExpandFill();
        _makingUnitsContainer.AddChild(_makingUnits.ItemList);

        var templates = regime.GetUnitTemplates(client.Data);
        _templates = new ItemListToken<UnitTemplate>(
            templates,
            t => t.Name,
            t => t.GetMaxPowerTroop(client.Data).Icon.Texture,
            (int)med,
            false
        );
        _templates.JustSelected += () => SetTemplateInfo(client);
        _templates.ItemList.ExpandFill();
        _templatesContainer.AddChild(_templates.ItemList);
    }

    private void SetMakingUnitsInfo(Client c)
    {
        var regime = _getRegime();
        _makingUnitsInfo.ClearChildren();
        if (_makingUnits.Values.Count != 1) return;
        var value = _makingUnits.Values.First();
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;

        _makingUnitsInfo.AddChild(value.GetDisplay(c.Data));
        
        var cancelMakeBtn = _makingUnitsInfo.AddButton(
            "Cancel", () =>
            {
                if (_makingUnits is null || _makingUnits.Values.Count == 0)
                {
                    return;
                }

                var proc = new AggregateProcedure(
                    _makingUnits.Values.Select(p =>
                        new CancelMakeProjectProcedure(regime.MakeRef(),
                            p.Id)).ToArray());
                var inner = new SendMessageCommand(proc, player);
                var com = CallbackCommand.Construct(
                    inner, () =>
                    {
                        if (IsInstanceValid(_makingUnits.ItemList))
                        {
                            var unitProjects = regime.MakeQueue.Queue
                                .OfType<UnitMakeProject>();
                            _makingUnits.Reset(unitProjects);
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
        var template = _templates.Values.Count == 1
            ? _templates.Values.First()
            : null;
        if (template is null) return;
        _templateInfo.AddChild(template.GetDisplay(c.Data));
        var num = new NumSliderAndEntry("Amount", 1f, 1f, 100f, 1f);
        _templateInfo.AddChild(num);
        var makeBtn = _templateInfo.AddButton(
            "Make", () =>
            {
                if (_templates is null)
                {
                    return;
                }

                var template = _templates.Values.Count == 1
                    ? _templates.Values.First()
                    : null;
                if (template is null) return;
                
                var proj = UnitMakeProject.Construct(regime,
                    template, (int)num.Value);
                var inner = new StartMakeProjectCommand(proj, player);
                var com = CallbackCommand.Construct(
                    inner, () =>
                    {
                        if (IsInstanceValid(_makingUnits.ItemList))
                        {
                            var unitProjects = regime.MakeQueue.Queue
                                .OfType<UnitMakeProject>();
                            _makingUnits.Reset(unitProjects);
                        }
                    }, c);
                c.HandleCommand(com);
            });
    }
}