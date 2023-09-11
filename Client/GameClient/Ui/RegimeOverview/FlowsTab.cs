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
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var flowIds = data.Models.GetModels<Flow>().Values;
        var tick = data.BaseDomain.GameClock.Tick;
        
        foreach (var flow in flowIds)
        {
            var flowIn = regime.Flows.Get(flow).FlowIn;
            var flowOut = regime.Flows.Get(flow).FlowOut;
            
            var hbox = new HBoxContainer();
            
            hbox.AddChild(flow.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"In: {flowIn} ");
            hbox.CreateLabelAsChild($"Out: {flowOut} ");
            _container.AddChild(hbox);
        }
    }
}