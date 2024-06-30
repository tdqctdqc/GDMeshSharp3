
using System.Linq;
using Godot;

public partial class ArmyPanel : Panel
{
    private VBoxContainer _inner;
    public ArmyPanel(Client c)
    {
        SelfModulate = Colors.Black;
        CustomMinimumSize = new Vector2(300f, 600f);
        _inner = this.MakeScroll<VBoxContainer>(new Vector2(300f, 600f));
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
        mouseActionOptions.CustomMinimumSize = new Vector2(300f, 100f);
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
            _inner.AddWindowButton<FillArmyWindow>(
                "Fill Army", 
                () => FillArmyWindow.Get(army, c));
            foreach (var unit in army.Units.Entities(c.Data))
            {
                var display = unit.GetUnitDisplay(c.Data);
                _inner.AddChild(display);
            }
        }
    }
}