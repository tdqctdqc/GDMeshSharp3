
using Godot;

public partial class ArmyIconGraphic : Node2D
{
    private static QuadMesh _border, _group, 
        _regime, _health;
    private static float _iconSize = 15f;
    private static LabelSettings _labelSettings;
    private MeshInstance2D _borderColor, _groupColor,
        _regimeColor, _healthMesh;
    private TextureRect _flagRect;
    private Control _control;
    private Army _army;
    static ArmyIconGraphic()
    {
        
        _labelSettings = new LabelSettings();
        _labelSettings.FontSize = 25;
        _border = MeshExt.GetQuadMesh(Vector2.One * _iconSize);
        _group = MeshExt.GetQuadMesh(Vector2.One * (_iconSize - .2f));
        _regime = MeshExt.GetQuadMesh(Vector2.One * _iconSize * .8f);
        _health = MeshExt.GetQuadMesh(Vector2.One * _iconSize * .15f);
    }
    public ArmyIconGraphic()
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.ArmyIcon;
        
        
    }

    public void Initialize()
    {
        _borderColor = new MeshInstance2D();
        _borderColor.Mesh = _border;
        _borderColor.Modulate = Colors.Black;
        AddChild(_borderColor);

        _groupColor = new MeshInstance2D();
        _groupColor.Mesh = _group;
        AddChild(_groupColor);

        _regimeColor = new MeshInstance2D();
        _regimeColor.Mesh = _regime;
        AddChild(_regimeColor);

        _flagRect = new TextureRect();
        _flagRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        var flagHeightProportion = .4f;
        var flagSize = new Vector2(_iconSize * flagHeightProportion * 1.5f,
            _iconSize * flagHeightProportion);
        _flagRect.Size = flagSize;

        _flagRect.Position = -flagSize / 2f;
        AddChild(_flagRect);
        
        var healthMarkerPos = new Vector2(_iconSize * .35f, -_iconSize * .35f);

        _healthMesh = new MeshInstance2D();
        _healthMesh.Position = new Vector2(_iconSize * .3f, -_iconSize * .3f);
        _healthMesh.Mesh = _health;
        AddChild(_healthMesh);

        _control = new Control();
        
        _control.Size = _iconSize * Vector2.One;
        _control.Position = -_iconSize * Vector2.One / 2f;
        _control.GuiInput += e =>
        {
            if (e is InputEventMouseButton mb
                && mb.Pressed == false
                && _army is not null 
                && Game.I.Client.UiController.Mode is ArmyMode am)
            {
                am.SelectOrCycle(_army);
            }
            else
            {
                Game.I.Client.UiController.Mode.HandleInput(e);
            }
        };
        AddChild(_control);
    }
    public void Draw(Army army, Client c)
    {
        _army = army;
        c.QueuedUpdates.Enqueue(() =>
        {
            var health = army.GetHealth(c.Data);
            var healthRatio = Mathf.Clamp(health.X / health.Y, 0f, 1f);
        
            _healthMesh.Modulate = Colors.Red.Lerp(Colors.Green, healthRatio);
            _regimeColor.Modulate = army.Regime.Get(c.Data).GetUnitColor();
            _groupColor.Modulate = army.Color;
            var regime = army.Regime.Get(c.Data);
            _flagRect.Texture = regime.Template.Get(c.Data).Flag.Texture;
        });
    }
}