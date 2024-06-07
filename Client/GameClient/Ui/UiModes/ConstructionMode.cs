
using System.Linq;
using Godot;

public class ConstructionMode : UiMode
{
    private MouseOverHandler _mouseOver;
    public DefaultSettingsOption<Settlement> Settlement { get; private set; }
    public ConstructionMode(Client client) : base(client,
        "Construction")
    {
        Settlement = new DefaultSettingsOption<Settlement>("Settlement", null);
        
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
        }
    }
    
    private void TryBuild()
    {
        
    }
    public override void Enter()
    {
    }

    public override void Clear()
    {
    }
}