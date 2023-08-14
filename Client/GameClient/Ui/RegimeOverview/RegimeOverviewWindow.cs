using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeOverviewWindow : TabWindow
{
    private RegimeGeneralOverview _general;
    private RegimeConstructionOverview _construction;
    private RegimePeepsOverview _peeps;
    private RegimeWalletOverview _wallet;
    private RegimeFoodOverview _ag;
    private VBoxContainer _regimeTemplates;
    public RegimeOverviewWindow()
    {
        Size = new Vector2I(500, 500);

        _general = new RegimeGeneralOverview();
        AddTab(_general);
        
        _construction = new RegimeConstructionOverview();
        AddTab(_construction);

        _peeps = new RegimePeepsOverview();
        AddTab(_peeps);

        _wallet = new RegimeWalletOverview();
        AddTab(_wallet);

        _ag = new RegimeFoodOverview();
        AddTab(_ag);

        var scroll = new ScrollContainer();
        scroll.Name = "Regime Templates";
        scroll.Size = new Vector2(200f, 400f);
        _regimeTemplates = new VBoxContainer();
        _regimeTemplates.Size = new Vector2(200f, 400f);
        scroll.AddChild(_regimeTemplates);
        AddTab(scroll);
        Hide();
    }
    public void Setup(Regime regime, Data data)
    {
        _general.Setup(regime, data);
        _construction.Setup(regime, data);
        _peeps.Setup(regime, data);
        _wallet.Setup(regime, data);
        _ag.Setup(regime, data);
        _regimeTemplates.ClearChildren();
        foreach (var kvp in data.Models.Cultures.Models)
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