
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyPanel : Panel
{
    private VBoxContainer _inner;
    private PolyPanel()
        : base()
    {
    }
    public PolyPanel(Client c) 
    {
        _inner = this.MakeScroll<VBoxContainer>(new Vector2(300f, 600f));
        SelfModulate = Colors.Black;
        
        var list = c.Data.Models.Buildings.GetList();
        var mode = c.UiController.ModeOption.Options
            .OfType<PolyMode>()
            .First();
        mode.Poly.SettingChanged.SubscribeForNode(
            n => Set(mode, c.Data),
            this);
        mode.Cell.SettingChanged.SubscribeForNode(
            n => Set(mode, c.Data),
            this);
    }

    private void Set(PolyMode mode, Data d)
    {
        _inner.ClearChildren();
        var poly = mode.Poly.Value;
        if (poly == null)
        {
            _inner.CreateLabelAsChild("No poly");
            return;
        }
        
        _inner.CreateLabelAsChild("Poly " + poly.Id);
        
        _inner.CreateLabelAsChild("Roughness " + poly.Roughness.RoundTo2Digits());
        

        
        if (mode.Cell.Value is LandCell l
                && l.GetSettlement(d) is Settlement s)
        {
            foreach (var (model, count) 
                     in s.Buildings.GetEnumerableModel(d))
            {
                var label = model.Icon
                    .GetLabeledIcon<HBoxContainer>(
                        $"{model.Name}: {count}", 40f);
                _inner.AddChild(label);
            }
        }
    }
}