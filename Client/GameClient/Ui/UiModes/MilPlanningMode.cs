
using System.Linq;
using Godot;

public class MilPlanningMode : UiMode
{
    private MouseOverHandler _mouseOver;
    public DefaultSettingsOption<Alliance> Alliance { get; private set; }
    public DefaultSettingsOption<IDeploymentNode> DeploymentNode { get; private set; }
    
    private MapOverlayDrawer _cellOverlay, _plansOverlay;

    public MilPlanningMode(Client client) : base(client,
        "Military Planning")
    {
        _mouseOver = new MouseOverHandler(client.Data);
        _mouseOver.ChangedCell += c => 
        {
            _cellOverlay.Clear();
            _mouseOver.Highlight(_cellOverlay);
        };
        Alliance = new DefaultSettingsOption<Alliance>("Alliance", null);
        Alliance.SettingChanged.Subscribe(n =>
        {
            var alliance = n.newVal;
            if (alliance is null || alliance.Leader.Get(_client.Data).IsPlayerRegime(_client.Data))
            {
                DeploymentNode.Set(null);
            }
            else
            {
                var root = alliance.GetAi(client.Data).Military.Deployment.Root;
                DeploymentNode.Set(root);
            }
            Draw(client);
        });

        DeploymentNode = new DefaultSettingsOption<IDeploymentNode>("Deployment Node",
            null);
        DeploymentNode.SettingChanged.Subscribe(n =>
        {
            Draw(client);
        });


    }

    private void Draw(Client c)
    {
        _plansOverlay.Clear();
        var node = DeploymentNode.Value;
        if (node is null) return;
        var relTo = node.GetCharacteristicCell(c.Data).GetCenter();
        _plansOverlay.Draw(mb => node.Draw(mb, relTo, c.Data), relTo);
    }
    public override void Process(float delta)
    {
        _mouseOver.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left
            && mb.Pressed == false)
        {
            var cell = _mouseOver.MouseOverCell;
            Alliance.Set(cell.Controller.Get(_client.Data)?.GetAlliance(_client.Data));
        }
    }

    public override void Enter()
    {
        var mg = _client.GetComponent<MapGraphics>();
        _plansOverlay = mg.GetOverlay(LayerOrder.Highlighter);
        _cellOverlay = mg.GetOverlay(LayerOrder.Highlighter);
    }


    
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.RemoveOverlay(_plansOverlay);
        mg.RemoveOverlay(_cellOverlay);
    }
}