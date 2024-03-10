
using System.Linq;
using Godot;

public partial class UnitPanel : ScrollPanel
{
    private Label _power, _regime, _id;
    private ColorRect _healthRect;
    
    private VBoxContainer _troops;
    private UnitPanel()
        : base()
    {
    }
    public UnitPanel(Client c) 
        : base(new Vector2(300f, 600f), Colors.Black)
    {
        _regime = new Label();
        Inner.AddChild(_regime);
        _id = new Label();
        Inner.AddChild(_id);


        var hbox = new HBoxContainer();
        _healthRect = new ColorRect();
        _healthRect.CustomMinimumSize = Vector2.One * 20f;
        _power = new Label();
        hbox.AddChild(_healthRect);
        hbox.AddChild(_power);
        Inner.AddChild(hbox);
        
        
        _troops = new VBoxContainer();
        Inner.AddChild(_troops);
        var mode = c.UiController.ModeOption.Options
            .OfType<UnitMode>()
            .First();
        mode.Unit.SettingChanged.SubscribeForNode(
            n => DrawForUnit(n.newVal, c.Data),
            this);
        DrawForUnit(mode.Unit.Value, c.Data);
    }

    private void DrawForUnit(Unit u, Data d)
    {
        _power.Text = "";
        _regime.Text = "";
        _id.Text = "";
        _troops.ClearChildren();
        _healthRect.Visible = false;
        if (u is null)
        {
            return;
        }
        var health = u.GetHealth(d);
        var healthRatio = health.X / health.Y;
        _healthRect.Visible = true;
        _healthRect.Modulate = Colors.Red.Interpolate(Colors.Green, healthRatio);
        _power.Text = $"{(int)health.X} / {(int)health.Y} Power Points";
        _regime.Text = u.Regime.Entity(d).Name;
        _id.Text = "Id: " + u.Id;
        foreach (var (troop, num) in u.Troops.GetEnumerableModel(d))
        {
            var labeled = troop.Icon.GetLabeledIcon<HBoxContainer>(
                num.ToString(), 30f);
            _troops.AddChild(labeled);
        }
    }
}