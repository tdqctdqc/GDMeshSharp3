
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
        _inner = margin.MakeScrollChild<VBoxContainer>(out var scroll);
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
            _inner.CreateLabelAsChild($"Units: {army.Units.Count()}");
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


            var regime = army.Regime.Get(c.Data);
            if (regime.IsPlayerRegime(c.Data) == false)
            {
                var root = regime.GetAi(c.Data).Military
                    .Deployment.Root;
                if (root is not null)
                {
                    var assgn = root
                        .GetDescendentAssignments()
                        .FirstOrDefault(a => a.Armies.Contains(army.MakeRef()));
                    if (assgn is not null)
                    {
                        _inner.AddButton($"{assgn.ToString()}", () =>
                        {
                            var milPlanMode = c.UiController.ModeOption.Choose<MilPlanningMode>();
                            milPlanMode.DeploymentNode.Set(assgn);
                        });
                    }
                }
            }
            
            var tree = army.GetTree(0, c);
            tree.ExpandFill();
            _inner.AddChild(tree);
        }
    }
}