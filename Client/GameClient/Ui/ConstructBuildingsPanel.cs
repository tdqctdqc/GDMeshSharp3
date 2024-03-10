
using System.Linq;
using Godot;

public partial class ConstructBuildingsPanel : ScrollPanel
{
    private VBoxContainer _info;
    private ConstructBuildingsPanel()
        : base()
    {
    }
    public ConstructBuildingsPanel(Client c) 
        : base(new Vector2(300f, 600f), Colors.Black)
    {
        var list = c.Data.Models.Buildings.GetList();
        var setting = c.UiController.ModeOption.Options
            .OfType<ConstructionMode>()
            .First().Setting;
        var menu = setting
            .GetControlInterfaceIcon(m => m.Icon.Texture,
            Vector2I.One * 50);
        Inner.AddChild(menu);
        _info = new VBoxContainer();
        Inner.AddChild(_info);
        setting.SettingChanged.SubscribeForNode(v =>
        {
            DrawInfo(v.newVal, c.Data);
        }, this);
    }

    private void DrawInfo(BuildingModel model, Data d)
    {
        _info.ClearChildren();
        var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer
            .Regime.Entity(d);
        if (playerRegime == null) return;
        
        _info.CreateLabelAsChild("Costs");
        if (model.Makeable.IndustrialCost > 0f)
        {
            var industrial = d.Models.Flows.IndustrialPower;
            industrial.Icon
                .GetLabeledIcon<HBoxContainer>(
                    $"{playerRegime.Flows.Flows[industrial.Id].Net()} / {model.Makeable.IndustrialCost}",
                    30f);
        }
        foreach (var (item, count) in model.Makeable.ItemCosts.GetEnumerableModel(d))
        {
            _info.AddChild(item.Icon.GetLabeledIcon<HBoxContainer>(
                $"{playerRegime.Items.Get(item)} / {count}", 30f));
        }
    }
}