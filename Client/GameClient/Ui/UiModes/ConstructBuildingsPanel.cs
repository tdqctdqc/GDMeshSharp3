
using System.Linq;
using Godot;

public partial class ConstructBuildingsPanel : Panel
{
    private VBoxContainer _info;
    private ConstructBuildingsPanel()
        : base()
    {
    }
    public ConstructBuildingsPanel(Client c) 
    {
        var inner = this.MakeScroll<VBoxContainer>(new Vector2(300f, 600f));
        this.SelfModulate = Colors.Black;
        
        var list = c.Data.Models.Buildings.GetList();
        var setting = c.UiController.ModeOption.Options
            .OfType<ConstructionMode>()
            .First().Building;
        var menu = setting
            .GetControlInterfaceIcon(m => m.Icon.Texture,
            Vector2I.One * 50);
        inner.AddChild(menu);
        _info = new VBoxContainer();
        inner.AddChild(_info);
        setting.SettingChanged.SubscribeForNode(v =>
        {
            DrawInfo(v.newVal, c.Data);
        }, this);
    }

    private void DrawInfo(SettlementBuildingModel model, Data d)
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
                    $"{playerRegime.Stock.Stock.Get(item)} / {count}", 30f));
            }
            else
            {
                _info.AddChild(NodeExt.CreateLabel(
                    $"{playerRegime.Stock.Stock.Get(item)} / {count}"));
            }
        }
    }
}