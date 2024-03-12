
using System.Linq;
using Godot;

public class ConstructionMode : UiMode
{
    public ListSettingsOption<BuildingModel> Setting { get; private set; }
    private MouseOverHandler _mouseOver;
    private MeshInstance2D _mesh;
    private Label _errorLabel;
    public ConstructionMode(Client client) : base(client,
        "Construction")
    {
        var list = client.Data.Models.Buildings.GetList();
        Setting = new ListSettingsOption<BuildingModel>(
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
        _mesh.Texture = Setting.Value.Icon.Texture;
        var model = Setting.Value;
        var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localPlayerRegime = localPlayer.Regime.Get(_client.Data);
        if (localPlayerRegime == null) return;
        // var proc = StartConstructionProcedure
        //     .Construct(model.MakeRef(),
        //         _mouseOver.MouseOverCell.Id,
        //         localPlayerRegime.MakeRef(),
        //         _client.Data);
        //
        // if (proc.Valid(_client.Data, out string error))
        // {
        //     _mesh.Modulate = Colors.White;
        //     _errorLabel.Text = "";
        // }
        // else
        // {
        //     _mesh.Modulate = new Color(Colors.White, .5f);
        //     _errorLabel.Text = error;
        // }
        
        _client.GetComponent<MapGraphics>().Segmenter
            .AddElement(_mesh, _mouseOver.MouseOverCell.GetCenter());
    }
    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left
            && mb.Pressed == false)
        {
            TryBuild();
        }
    }

    private void TryBuild()
    {
        var model = Setting.Value;
        var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localPlayerRegime = localPlayer.Regime.Get(_client.Data);
        if (localPlayerRegime is not null)
        {
            // var proc = StartConstructionProcedure
            //     .Construct(model.MakeRef(),
            //         _mouseOver.MouseOverCell.Id,
            //         localPlayerRegime.MakeRef(),
            //         _client.Data);
            // var com = new DoProcedureCommand(proc, localPlayer.PlayerGuid);
            // _client.HandleCommand(com);
        }
    }
    public override void Enter()
    {
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