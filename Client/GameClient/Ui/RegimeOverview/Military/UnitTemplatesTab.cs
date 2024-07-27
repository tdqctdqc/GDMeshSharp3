using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class UnitTemplatesTab : HBoxContainer, IUiDrawable
{
    private ItemListToken<UnitTemplate> _templates;
    private ItemListToken<TroopDomain> _domain;
    private ItemListToken<TroopType> _troopTypes;
    private VBoxContainer _selectedTemplateInfo;
    private Func<Regime> _getRegime;

    public UnitTemplatesTab(Func<Regime> getRegime)
    {
        Name = "Templates";
        _getRegime = getRegime;
    }

    public void Draw(Client client)
    {
        this.ClearChildren();
        var leftSide = new VBoxContainer();
        leftSide.ExpandFill(1);
        AddChild(leftSide);
        var regime = _getRegime();
        var med = client.Settings.MedIconSize.Value;

        var domains = client.Data.Models.GetModels<TroopDomain>();
        
        _domain = new ItemListToken<TroopDomain>(
            domains,
            d => d.Name,
            false);
        _domain.ItemList.ExpandFill();
        _domain.SelectAt(0);
        _domain.JustSelected += () =>
        {
            _templates.Reset(GetValidTemplates(client));
            _troopTypes.Reset(GetValidTroopTypes(client));
        };
        
        
        _templates = new ItemListToken<UnitTemplate>(
            GetValidTemplates(client),
            u => u.Name,
            u => u.GetIcon(client.Data).Texture,
            (int)med,
             false
        );
        _templates.JustSelected += () =>
        {
            if (IsInstanceValid(this))
            {
                DrawTemplateInfo(client);
            }
        };
        
        _templates.ItemList.ExpandFill();
        var templateBox = ContainerExt.MakeScroll<VBoxContainer>(out var templateScroll);
        templateScroll.ExpandFill();
        templateBox.ExpandFill();
        templateBox.AddChild(_templates.ItemList);
        leftSide.AddChild(templateScroll);

        var addNewTemplate = leftSide.AddButton("Add New Template",
            () =>
            {
                var inner = new CreateUnitTemplateCommand(
                    _domain.Selected.Single().MakeRef(),
                    client.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
                var com = CallbackCommand.Construct(inner, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        _templates.Reset(GetValidTemplates(client));
                    }
                }, client);
                client.HandleCommand(com);
            });
        var renameTemplate = leftSide.AddButton("Rename Template",
            () =>
            {
               TextPromptWindow.Open(client, 
                    "New template name: ",
                    s =>
                    {
                        var inner = new RenameTemplateProcedure(
                            _templates.Selected.Single().MakeRef(),
                            s);
                        var inner2 = new SendMessageCommand(inner, client.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);
                        var com = CallbackCommand.Construct(inner2, () =>
                        {
                            if (IsInstanceValid(this))
                            {
                                _templates.Reset(GetValidTemplates(client));
                            }
                        }, client);
                        client.HandleCommand(com);
                    });
                
            });
        
        leftSide.AddChild(_domain.ItemList);

        _selectedTemplateInfo = new VBoxContainer();
        _selectedTemplateInfo.ExpandFill(3);
        AddChild(_selectedTemplateInfo);

        var rightSide = new VBoxContainer();
        rightSide.ExpandFill(1);
        AddChild(rightSide);

        _troopTypes = new ItemListToken<TroopType>(
            GetValidTroopTypes(client),
            t => t.Name,
            t => t.Icon.Texture,
            (int)med,
            false);
        _troopTypes.ItemList.ExpandFill();
        rightSide.AddChild(_troopTypes.ItemList);
        var troopNumSlider = new NumSliderAndEntry("Num Troop",
            0f, 0f, 1000f, 1f);
        rightSide.AddChild(troopNumSlider);
        rightSide.AddButton("Set Num Troop",
            () =>
            {
                if (_templates.Selected.Count == 0 
                    || _troopTypes.Selected.Count == 0) return;
                var inner = new ChangeTemplateTroopAmountProcedure(
                    _templates.Selected.Single().MakeRef(),
                    _troopTypes.Selected.Single().MakeRef(),
                    troopNumSlider.Value);
                var inner2 = new SendMessageCommand(inner, client.Data.BaseDomain.PlayerAux.LocalPlayer.PlayerGuid);

                var com = CallbackCommand.Construct(inner2, () =>
                {
                    if (IsInstanceValid(this))
                    {
                        DrawTemplateInfo(client);
                    }
                }, client);
                client.HandleCommand(com);
            });
    }

    private IEnumerable<UnitTemplate> GetValidTemplates(Client client)
    {
        var domain = _domain.Selected.Single();
        var regime = _getRegime();
        return regime.GetUnitTemplates(client.Data).Where(t => t.Domain == domain);
    }
    private IEnumerable<TroopType> GetValidTroopTypes(Client client)
    {
        var domain = _domain.Selected.Single();
        var regime = _getRegime();
        return client.Data.Models.GetModels<Troop>()
            .Select(t => t.TroopType)
            .Distinct()
            .Where(t => t.TroopDomain == domain);

    }
    private void DrawTemplateInfo(Client client)
    {
        _selectedTemplateInfo.ClearChildren();
        if (_templates.Selected.Count != 1) return;
        var template = _templates.Selected.Single();
        var large = client.Settings.LargeIconSize.Value;
        foreach (var (troop, value) in template.Troops.GetEnumModel(client.Data))
        {
            var label = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"{troop.Name}: {value}", large);
            _selectedTemplateInfo.AddChild(label);
        }
    }
}