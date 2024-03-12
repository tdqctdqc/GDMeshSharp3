
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
            .Regime.Get(d);
        if (playerRegime == null) return;
        
        _info.CreateLabelAsChild("Costs");
        foreach (var (item, count) in model.Makeable.BuildCosts.GetEnumerableModel(d))
        {
            if (item is IIconed iconed)
            {
                _info.AddChild(iconed.Icon.GetLabeledIcon<HBoxContainer>(
                    $"{playerRegime.Store.Get(item)} / {count}", 30f));
            }
            else
            {
                _info.AddChild(NodeExt.CreateLabel(
                    $"{playerRegime.Store.Get(item)} / {count}"));
            }
        }
    }
}