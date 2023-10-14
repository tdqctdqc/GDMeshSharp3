using Godot;

namespace Ui.RegimeOverview;

public partial class TroopReserveTab : ScrollContainer
{
    private VBoxContainer _container;
    public TroopReserveTab()
    {
        Name = "Troop Reserve";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var troops = data.Models.GetModels<Troop>().Values;
        var tick = data.BaseDomain.GameClock.Tick;
        
        foreach (var troop in troops)
        {
            var amt = regime.TroopReserve.Get(troop);
            var hbox = new HBoxContainer();
            hbox.AddChild(troop.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"Amount: {amt} ");
            _container.AddChild(hbox);
        }
    }
}