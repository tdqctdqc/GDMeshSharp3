using System;
using System.Linq;
using Godot;

namespace Ui.MilitaryWindow;

public partial class MakeUnitsTab : HBoxContainer, IUiDrawable
{
    private VBoxContainer _makingUnitsContainer,
        _makingUnitsInfo,
        _templateInfo,
        _templatesContainer,
        _desiredInfo;

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
            p => $"{p.Template.Get(client.Data).Name} {p.Fulfilled} / {p.Amount}",
            p => p.GetIcon(client.Data).Texture,
            (int)med,
            true);
        _makingUnits.JustSelected += () => SetMakingUnitsInfo(client);
        _makingUnits.ItemList.ExpandFill();
        _makingUnitsContainer.AddChild(_makingUnits.ItemList);

        var templates = regime.GetUnitTemplates(client.Data);
        _templates = new ItemListToken<UnitTemplate>(
            templates,
            t => t.Name,
            t => t.GetIcon(client.Data).Texture,
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
        if (_makingUnits.Selected.Count != 1) return;
        var value = _makingUnits.Selected.First();
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;

        _makingUnitsInfo.AddChild(value.GetDisplay(c.Data));
        
        var cancelMakeBtn = _makingUnitsInfo.AddButton(
            "Cancel", () =>
            {
                if (_makingUnits is null || _makingUnits.Selected.Count == 0)
                {
                    return;
                }

                var proc = new AggregateProcedure(
                    _makingUnits.Selected.Select(p =>
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

        if (regime.IsPlayerRegime(c.Data) == false)
        {
            var totals = regime
                .GetAi(c.Data).Military.ForceComposition
                .GetCurrentAndNeededTotals(c.Data);
            foreach (var (key, value) in totals)
            {
                _templateInfo.CreateLabelAsChild($"{key.Name}: {value.Y} / {value.X}");
            }
        }
        
        
        var player = c.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid;
        var template = _templates.Selected.Count == 1
            ? _templates.Selected.First()
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

                var template = _templates.Selected.Count == 1
                    ? _templates.Selected.First()
                    : null;
                if (template is null) return;
                
                var proj = UnitMakeProject.Construct(regime,
                    template, (int)num.Value, c.Data);
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