
using System.Linq;
using Godot;

public class ConstructionMode : UiMode
{
    private MouseOverHandler _mouseOver;
    private MeshInstance2D _selectedCellGraphic;
    public DefaultSettingsOption<Settlement> Settlement { get; private set; }
    public ConstructionMode(Client client) : base(client,
        "Construction")
    {
        Settlement = new DefaultSettingsOption<Settlement>("Settlement", null);
        Settlement.SettingChanged.Subscribe(v =>
        {
            if (v.newVal is null)
            {
                _selectedCellGraphic.Mesh = null;
            }
            else
            {
                var mb = new MeshBuilder();
                var cell = v.newVal.Cell.Get(client.Data);
                mb.DrawPolygon(cell.RelBoundary,
                    Colors.Yellow.Tint(.25f));
                _selectedCellGraphic.Mesh = mb.GetMesh();
                client.GetComponent<MapGraphics>().Segmenter
                    .AddElement(_selectedCellGraphic, cell.RelTo);
            }
        });
        var list = client.Data.Models.Buildings.GetList();
        _mouseOver = new MouseOverHandler(client.Data);
        _mouseOver.ChangedCell += c =>
        {
            Highlight();
        };

    }

    public override void Process(float delta)
    {
        _mouseOver.Process(delta);
    }

    
    private void Highlight()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        var localPlayer = _client.Data.BaseDomain.PlayerAux.LocalPlayer;
        var localPlayerRegime = localPlayer.Regime.Get(_client.Data);
        if (localPlayerRegime == null) return;
        
        
    }
    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left
            && mb.Pressed == false)
        {
            var cell = _mouseOver.MouseOverCell;
            if (cell is not null
                && cell.GetSettlement(_client.Data) is Settlement s)
            {
                Settlement.Set(s);
            }
            else
            {
                Settlement.Set(null);
            }
        }
    }
    
    public override void Enter()
    {
        _selectedCellGraphic?.QueueFree();
        _selectedCellGraphic = new MeshInstance2D();
        _selectedCellGraphic.ZIndex = (int)LayerOrder.Highlighter;
        _selectedCellGraphic.ZAsRelative = false;
    }

    public override void Clear()
    {
        _selectedCellGraphic.QueueFree();
    }
}