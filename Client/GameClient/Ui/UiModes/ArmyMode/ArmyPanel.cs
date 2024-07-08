
using System.Linq;
using Godot;
using Ui.MilitaryWindow;
using Ui.RegimeOverview;

public partial class ArmyPanel : PanelContainer
{
    private VBoxContainer _inner;
    public ArmyPanel(Client c)
    {
        var margin = new MarginContainer();
        AddChild(margin);
        SelfModulate = Colors.Black;
        _inner = margin.MakeScroll<VBoxContainer>();
        _inner.ExpandFill();
        var mode = c.UiController.ModeOption.Options
            .OfType<ArmyMode>().First();
        mode.Army.SettingChanged.SubscribeForNode(v => Draw(v.newVal, c), this);
        Draw(null, c);
    }
    private ArmyPanel() { }

    private void Draw(Army army, Client c)
    {
        _inner.ClearChildren();
        var mode = c.UiController.ModeOption.Options
            .OfType<ArmyMode>().First();
        var mouseActionOptions = mode.MouseActions
            .GetControlInterface();
        _inner.AddChild(mouseActionOptions);
        if (army is not null)
        {
            _inner.CreateLabelAsChild(army.Regime.Get(c.Data).Name);
            _inner.CreateLabelAsChild(army.Id.ToString());
            _inner.CreateLabelAsChild(army.LineMission.GetDescription(c.Data));
            foreach (var order in army.OtherOrders)
            {
                _inner.CreateLabelAsChild(order.GetDescription(c.Data));
            }
            _inner.AddButton(
                "Fill Army",
                () =>
                {
                    var w = RegimeOverviewWindow.Open(
                        army.Regime.Get(c.Data),
                        c);
                    var m = w.OpenTab<MilitaryTab>();
                    var a = m.OpenTab<ArmiesTab>();
                    a.SelectArmy(army);
                });
            var tree = army.GetTree(0, c);
            tree.ExpandFill();
            _inner.AddChild(tree);
        }
    }
}