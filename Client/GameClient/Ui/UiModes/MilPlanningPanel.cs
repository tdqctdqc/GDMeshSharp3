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
            var pos = n.GetCharacteristicCell(d).GetCenter();
            c.Cam().SetPos(pos);
        };
    }

    private void DrawDeploymentNodeInfo(IDeploymentNode node, Client c)
    {
        if (_deploymentNodeInfo is null) return;
        var mode = c.UiController.ModeOption.Options
            .OfType<MilPlanningMode>()
            .First();
        var regime = mode.Regime.Value;
        if (regime is null || regime.IsPlayerRegime(c.Data))
        {
            return;
        }

        var ai = regime.GetAi(c.Data).Military.Deployment;
        
        
        
        _deploymentNodeInfo.ClearChildren();
        if (node is null) return;
        _deploymentNodeInfo.CreateLabelAsChild(node.ToString());
        _deploymentNodeInfo.CreateLabelAsChild($"PP Need: {node.GetPowerPointNeed(c.Data)}");
        _deploymentNodeInfo.CreateLabelAsChild($"PP Assigned: {node.GetPowerPointsAssigned(c.Data)}");
        
        
        var allNodes = ai.Root
            .GetDescendentNodes().OfType<DeploymentBranch>().ToArray();
        var parent = allNodes
            .FirstOrDefault(p => p.SubBranches.Contains(node)
                                 || p.Assignments.Contains(node));
        if (parent is not null)
        {
            _deploymentNodeInfo.AddButton("Parent: " + parent.ToString(),
                () =>
                {
                    mode.DeploymentNode.Set(parent);
                });
        }


        if (node is DeploymentBranch b)
        {
            if (b.SubBranches.Any())
            {
                _deploymentNodeInfo.CreateLabelAsChild("Sub Branches");
                var subInfo = _deploymentNodeInfo.MakeScrollChild<VBoxContainer>(
                    out var subScroll);
                subScroll.ExpandFill();
                subScroll.CustomMinimumSize = new Vector2(0f, 100f);
                
                foreach (var sub in b.SubBranches)
                {
                    subInfo.AddButton(sub.ToString(),
                        () => mode.DeploymentNode.Set(sub));
                }
            }
            
            if (b.Assignments.Any())
            {
                _deploymentNodeInfo.CreateLabelAsChild("Assignments");
                var assgnInfo = _deploymentNodeInfo.MakeScrollChild<VBoxContainer>(
                    out var assgnScroll);
                assgnScroll.ExpandFill();
                assgnScroll.CustomMinimumSize = new Vector2(0f, 100f);
                
                foreach (var assgn in b.Assignments)
                {
                    assgnInfo.AddButton(assgn.ToString(),
                        () => mode.DeploymentNode.Set(assgn));
                }
            }
        }
        
        
                
        
        if (node is ArmyAssignment g)
        {
            _deploymentNodeInfo.CreateLabelAsChild($"Armies: {g.Armies.Count()}");
            var armyScroll = _deploymentNodeInfo.MakeScrollChild<VBoxContainer>(
                out var armyScrollContainer);
            armyScrollContainer.ExpandFill();
            armyScrollContainer.CustomMinimumSize = new Vector2(0f, 100f);

            foreach (var gArmy in g.Armies)
            {
                var army = gArmy.Get(c.Data);
                var button = ButtonExt.GetButton(() =>
                {
                    var pos = army.GetCell(c.Data).GetCenter();
                    c.Cam().SetPos(pos);
                });
                button.Text = "Army " + army.Id.ToString();
                button.Modulate = army.Color;
                armyScroll.AddChild(button);
            }
        }
    }
}