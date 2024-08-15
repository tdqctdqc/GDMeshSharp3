using System.Linq;
using Godot;

public partial class MilPlanningPanel : PanelContainer
{
    private VBoxContainer _inner, _deploymentNodeInfo;
    private ItemListToken<IDeploymentNode> _deploymentNodes;

    public MilPlanningPanel(Client c)
    {
        var margin = new MarginContainer();
        margin.FullRect();
        margin.ExpandFill();
        AddChild(margin);
        _inner = margin.MakeScrollChild<VBoxContainer>(out var scroll);
        _inner.FullRect();
        _inner.ExpandFill();
        SelfModulate = Colors.Black;
        var mode = c.UiController.ModeOption.Options
            .OfType<MilPlanningMode>()
            .First();
        mode.Regime.SettingChanged.SubscribeForNode(
            n => Set(c, mode),
            this);
        mode.DeploymentNode.SettingChanged.SubscribeForNode(
            n =>
            {
                DrawDeploymentNodeInfo(n.newVal, c);
            },
            this);
        Set(c, mode);
    }
    
    private void Set(Client c, MilPlanningMode mode)
    {
        var d = c.Data;
        _inner.ClearChildren();
        _deploymentNodeInfo = null;
        var regime = mode.Regime.Value;
        if (regime == null || regime.IsPlayerRegime(d))
        {
            _inner.CreateLabelAsChild("No alliance");
            return;
        }

        var ai = regime.GetAi(d);
        _deploymentNodeInfo = new VBoxContainer();
        _inner.AddChild(_deploymentNodeInfo);
        var root = ai.Military
            .Deployment.Root;
        var nodes = root.Yield().Concat(root.GetDescendentNodes());
        
        _deploymentNodes = new ItemListToken<IDeploymentNode>(
            nodes,
            n => n.ToString(),
            false
        );
        _deploymentNodes.ItemList.ExpandFill();
        _inner.AddChild(_deploymentNodes.ItemList);
        _deploymentNodes.JustSelected += () =>
        {
            var n = _deploymentNodes.Selected.Single();
            mode.DeploymentNode.Set(n);
        };
    }

    private void DrawDeploymentNodeInfo(IDeploymentNode node, Client c)
    {
        if (_deploymentNodeInfo is null) return;
        _deploymentNodeInfo.ClearChildren();
        if (node is null) return;
        _deploymentNodeInfo.CreateLabelAsChild(node.ToString());

        if (node is ArmyAssignment g)
        {
            _deploymentNodeInfo.CreateLabelAsChild($"Armies: {g.Armies.Count()}");
            foreach (var gArmy in g.Armies)
            {
                var army = gArmy.Get(c.Data);
                var label = new Label();
                label.Text = "Army " + army.Id.ToString();
                label.Modulate = army.Color;
                _deploymentNodeInfo.AddChild(label);
            }
        }
    }
}