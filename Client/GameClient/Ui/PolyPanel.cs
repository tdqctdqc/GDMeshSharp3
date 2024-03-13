
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyPanel : ScrollPanel
{
    private PolyPanel()
        : base()
    {
    }
    public PolyPanel(Client c) 
        : base(new Vector2(300f, 600f), Colors.Black)
    {
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
        Inner.ClearChildren();
        var poly = mode.Poly.Value;
        if (poly == null)
        {
            Inner.CreateLabelAsChild("No poly");
            return;
        }
        
        Inner.CreateLabelAsChild("Poly " + poly.Id);
        
        Inner.CreateLabelAsChild("Roughness " + poly.Roughness.RoundTo2Digits());
        

        if (poly.GetBuildings(d) is List<MapBuilding> buildings)
        {
            foreach (var (model, count) 
                     in poly.GetBuildings(d)
                         .Select(b => b.Model.Get(d))
                         .GetCounts())
            {
                var label = model.Icon
                    .GetLabeledIcon<HBoxContainer>(
                        $"{model.Name}: {count}", 40f);
                Inner.AddChild(label);
            }
        }
    }
}