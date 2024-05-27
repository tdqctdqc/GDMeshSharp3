
using System.Linq;
using Godot;

public partial class ArmyPanel : Panel
{
    private Container _inner;
    public ArmyPanel(Client c)
    {
        _inner = this.MakeScroll<VBoxContainer>(new Vector2(300f, 600f));
        SelfModulate = Colors.Black;
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
        var mouseActionOptions = mode.MouseActions.GetControlInterface();
        _inner.AddChild(mouseActionOptions);
        if (army is not null)
        {
            _inner.CreateLabelAsChild(army.Regime.Get(c.Data).Name);
            _inner.CreateLabelAsChild(army.Id.ToString());
            

            var order = army.GroupOrder;
            _inner.CreateLabelAsChild(order is not null 
                ? order.GetDescription(c.Data)
                : "No Orders");
            
            _inner.AddWindowButton<FillArmyWindow>(
                "Fill Army", w => w.Setup(army));
            
            foreach (var unit in army.Units.Items(c.Data))
            {
                var display = unit.GetUnitDisplay(c.Data);
                _inner.AddChild(display);
            }
        }
    }
}