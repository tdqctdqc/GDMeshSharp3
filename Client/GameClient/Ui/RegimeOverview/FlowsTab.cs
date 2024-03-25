using Godot;

namespace Ui.RegimeOverview;

public partial class FlowsTab : ScrollContainer
{
    private VBoxContainer _container;
    public FlowsTab()
    {
        Name = "Flows";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        var flowIds = client.Data.Models.GetModels<Flow>().Values;
        var tick = client.Data.BaseDomain.GameClock.Tick;
        var iconSize = client.Settings.MedIconSize.Value;

        foreach (var flow in flowIds)
        {
            var avail = regime.Stock.Stock.Get(flow);
            var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                flow.Icon, $"Available: {avail}",
                iconSize);
            _container.AddChild(box);
        }
    }
}