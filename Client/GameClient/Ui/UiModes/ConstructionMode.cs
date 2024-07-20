
using System.Linq;
using Godot;

public class ConstructionMode : UiMode
{
    public DefaultSettingsOption<Cell> Cell { get; private set; }
    private MouseOverHandler _mouseOver;
    private MapOverlayDrawer _mouseOverlay, _selectedOverlay;
    public ConstructionMode(Client client) : base(client,
        "Construction")
    {
        Cell = new DefaultSettingsOption<Cell>("Cell", null);
        Cell.SettingChanged.Subscribe(v =>
        {
            _selectedOverlay.Clear();

            if (v.newVal is not null)
            {
                var mb = new MeshBuilder();
                _selectedOverlay.Draw(mb =>
                {
                    mb.DrawPolygon(v.newVal.RelBoundary,
                        Colors.Yellow.Tint(.25f));
                }, v.newVal.RelTo);
            }
        });
        
        _mouseOver = new MouseOverHandler(client.Data);
        _mouseOver.ChangedCell += c =>
        {
            _mouseOverlay.Clear();
            _mouseOver.Highlight(_mouseOverlay);
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
            Cell.Set(cell);
        }
    }
    
    public override void Enter()
    {
        var mg = _client.GetComponent<MapGraphics>();
        _mouseOverlay = mg.GetOverlay(LayerOrder.Highlighter);
        _selectedOverlay = mg.GetOverlay(LayerOrder.Highlighter);
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.RemoveOverlay(_mouseOverlay);
        mg.RemoveOverlay(_selectedOverlay);
    }
}