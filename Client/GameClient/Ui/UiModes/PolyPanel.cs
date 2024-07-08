
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyPanel : PanelContainer
{
    private VBoxContainer _inner;
    private PolyPanel()
        : base()
    {
    }
    public PolyPanel(Client c)
    {
        var margin = new MarginContainer();
        AddChild(margin);
        _inner = margin.MakeScroll<VBoxContainer>();
        SelfModulate = Colors.Black;
        var mode = c.UiController.ModeOption.Options
            .OfType<PolyMode>()
            .First();
        mode.Poly.SettingChanged.SubscribeForNode(
            n => Set(c, mode, c.Data),
            this);
        mode.Cell.SettingChanged.SubscribeForNode(
            n => Set(c, mode, c.Data),
            this);
    }

    private void Set(Client c, PolyMode mode, Data d)
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

        var cell = mode.Cell.Value;
        _inner.CreateLabelAsChild($"Cell: {cell.Id}");
        _inner.CreateLabelAsChild($"Landform: {cell.Landform.Get(d).Name}");
        _inner.CreateLabelAsChild($"Vegetation: {cell.Vegetation.Get(d).Name}");
        
        
        if (cell is LandCell l)
        {
            var peep = l.GetPeep(d);
            var med = c.Settings.MedIconSize.Value;
            _inner.CreateLabelAsChild($"Population: {peep.Size}");
            foreach (var (job, value) in peep.Employment.Counts.GetEnumModel(d))
            {
                var entry = job.Icon.GetLabeledIcon<HBoxContainer>(
                    $"{job.Name}: {value}", med);
                _inner.AddChild(entry);
            }
            if (l.GetSettlement(d) is Settlement s)
            {
                _inner.CreateLabelAsChild($"Settlement: {s.Name}");
                foreach (var (model, count) 
                         in s.Buildings.GetEnumModel(d))
                {
                    var label = model.Icon
                        .GetLabeledIcon<HBoxContainer>(
                            $"{model.Name}: {count}", med);
                    _inner.AddChild(label);
                }
            }
        }
    }
}