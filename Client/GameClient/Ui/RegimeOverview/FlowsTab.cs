using Godot;

namespace Ui.RegimeOverview;

public partial class FlowsTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public FlowsTab(RegimeOverviewWindow parent)
    {
        Name = "Flows";
        _parent = parent;
        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }

    private FlowsTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
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