
using System.Linq;
using Godot;

public class ConstructionMode : UiMode
{
    private MouseOverHandler _mouseOver;
    private MapOverlayDrawer _cellOverlay, _settlementOverlay;
    public DefaultSettingsOption<Settlement> Settlement { get; private set; }
    public ConstructionMode(Client client) : base(client,
        "Construction")
    {
        Settlement = new DefaultSettingsOption<Settlement>("Settlement", null);
        Settlement.SettingChanged.Subscribe(v =>
        {
            _settlementOverlay.Clear();
            
            if (v.newVal is not null)
            {
                var mb = new MeshBuilder();
                var cell = v.newVal.Cell.Get(client.Data);
                _settlementOverlay.Draw(mb =>
                {
                    mb.DrawPolygon(cell.RelBoundary,
                        Colors.Yellow.Tint(.25f));
                }, cell.GetCenter());
            }
        });
        _mouseOver = new MouseOverHandler(client.Data);
        _mouseOver.ChangedCell += c =>
        {
            _mouseOver.Highlight(_cellOverlay);
        };

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
        var mg = _client.GetComponent<MapGraphics>();
        _settlementOverlay = mg.GetOverlay(LayerOrder.Highlighter);
        _cellOverlay = mg.GetOverlay(LayerOrder.Highlighter);
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.RemoveOverlay(_settlementOverlay);
        mg.RemoveOverlay(_cellOverlay);
    }
}