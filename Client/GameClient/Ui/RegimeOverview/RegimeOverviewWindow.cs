using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeOverviewWindow : TabWindow
{
    private RegimeGeneralOverview _general;
    private RegimeConstructionOverview _construction;
    private RegimePeepsOverview _peeps;
    private RegimeProductionOverview _prod;
    private RegimeWalletOverview _wallet;
    private VBoxContainer _regimeTemplates;
    private Button _switchToRegime;
    public RegimeOverviewWindow()
    {
        Size = new Vector2I(500, 500);

        _general = new RegimeGeneralOverview();
        AddTab(_general);
        
        _construction = new RegimeConstructionOverview();
        AddTab(_construction);

        _peeps = new RegimePeepsOverview();
        AddTab(_peeps);

        _prod = new RegimeProductionOverview();
        AddTab(_prod);

        _wallet = new RegimeWalletOverview();
        AddTab(_wallet);

        _switchToRegime = new Button();
        var scroll = new ScrollContainer();
        scroll.Name = "Regime Templates";
        scroll.Size = new Vector2(200f, 400f);
        _regimeTemplates = new VBoxContainer();
        _regimeTemplates.Size = new Vector2(200f, 400f);
        scroll.AddChild(_regimeTemplates);
        AddTab(scroll);
        Hide();
    }
    public void Setup(Regime regime, ClientWriteKey key)
    {
        _general.Setup(regime, key);
        _construction.Setup(regime, key.Data);
        _peeps.Setup(regime, key.Data);
        _prod.Setup(regime, key.Data);
        _wallet.Setup(regime, key.Data);
        _regimeTemplates.ClearChildren();
        foreach (var kvp in key.Data.Models.Cultures.Models)
        {
            kvp.Value.RegimeTemplates.ForEach(rt =>
            {
                var entry = new HBoxContainer();
                
                var colBox = new VBoxContainer();
                var primRect = new ColorRect();
                primRect.Color = new Color(rt.PrimaryColor);
                primRect.CustomMinimumSize = new Vector2(10f, 50f);
                var secRect = new ColorRect();
                secRect.Color = new Color(rt.SecondaryColor);
                secRect.CustomMinimumSize = new Vector2(10f, 50f);
                colBox.AddChild(primRect);
                colBox.AddChild(secRect);
                entry.AddChild(colBox);
                
                var flagRect = new TextureRect();
                flagRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeight;
                flagRect.CustomMinimumSize = new Vector2(150f, 100f);
                flagRect.Texture = rt.Flag;
                entry.AddChild(flagRect);
                var l = new Label();
                l.Text = rt.Name;
                entry.AddChild(l);
                _regimeTemplates.AddChild(entry);
            });
        }
    }
}