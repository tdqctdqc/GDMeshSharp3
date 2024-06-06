
using System.Linq;
using Godot;

public class ConstructionMode : UiMode
{
    public ListSettingsOption<SettlementBuildingModel> Building { get; private set; }
    private MouseOverHandler _mouseOver;
    private MeshInstance2D _mesh;
    private Label _errorLabel;
    public ConstructionMode(Client client) : base(client,
        "Construction")
    {
        var list = client.Data.Models.Buildings.GetList();
        Building = new ListSettingsOption<SettlementBuildingModel>(
            "Building", list, 
            list.Select(m => m.Name).ToList());
        _mouseOver = new MouseOverHandler(client.Data);
        _mouseOver.ChangedCell += c => Highlight();
        
    }

    public override void Process(float delta)
    {
        _mouseOver.Process(delta);
    }
    private void Highlight()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        _mesh.Texture = Building.Value.Icon.Texture;
        var model = Building.Value;
        var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localPlayerRegime = localPlayer.Regime.Get(_client.Data);
        if (localPlayerRegime == null) return;
        
        _client.GetComponent<MapGraphics>().Segmenter
            .AddElement(_mesh, _mouseOver.MouseOverCell.GetCenter());
    }
    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Right
            && mb.Pressed == false)
        {
            TryBuild();
        }
    }
    
    private void TryBuild()
    {
        var model = Building.Value;
        var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localPlayerRegime = localPlayer.Regime.Get(_client.Data);
        if (model is not null
            && localPlayerRegime is not null
            && _mouseOver.MouseOverCell is Cell cell
            && cell.Controller.RefId == localPlayerRegime.Id
            && cell.GetSettlement(_client.Data) is Settlement s)
        {
            var project = PlayerBuildingMakeProject.Construct(
                s, localPlayerRegime, model);
            var com = new StartMakeProjectCommand(project, localPlayer.PlayerGuid);
            _client.HandleCommand(com);
        }
    }
    public override void Enter()
    {
        _mesh?.QueueFree();
        _mesh = new MeshInstance2D();
        var q = new QuadMesh();
        q.Size = Vector2.One * 30f;
        _mesh.Mesh = q;
        _mesh.Scale = new Vector2(1f, -1f);
        _mesh.ZIndex = (int)LayerOrder.Ui;
        
        _errorLabel = new Label();
        _errorLabel.ZIndex = 99;
        _errorLabel.Modulate = Colors.Red;
        _errorLabel.Scale = new Vector2(1f, -1f);
        _mesh.AddChild(_errorLabel);
    }

    public override void Clear()
    {
        _mesh.QueueFree();
        _mesh = null;
    }
}