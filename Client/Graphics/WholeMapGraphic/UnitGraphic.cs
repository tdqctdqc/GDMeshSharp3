
using System.Linq;
using Godot;

public partial class UnitGraphic : Node2D
{
    private MeshInstance2D 
        _borderColor,
        _groupColor,
        _regimeColor,
        _healthMesh;

    private TextureRect _troopRect, _flagRect;
    private static LabelSettings _labelSettings;
    private static QuadMesh _border, _group, _regime, 
        _health;
    private static float _iconSize = 15f;
    static UnitGraphic()
    {
        _labelSettings = new LabelSettings();
        _labelSettings.FontSize = 25;
        _border = MeshExt.GetQuadMesh(Vector2.One * _iconSize);
        _group = MeshExt.GetQuadMesh(Vector2.One * (_iconSize - .2f));
        _regime = MeshExt.GetQuadMesh(Vector2.One * _iconSize * .8f);
        _health = MeshExt.GetQuadMesh(Vector2.One * _iconSize * .15f);
    }
    private UnitGraphic() {}
    
    public UnitGraphic(Unit unit, Data data)
    {
        var regime = unit.Regime.Get(data);
        
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

        _flagRect = regime.Template.Get(data).Flag.
            GetTextureRect(_iconSize * .25f);
        _flagRect.Position = new Vector2(0f, _iconSize * .1f);
        AddChild(_flagRect);
        
        _troopRect = new TextureRect();
        _troopRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        _troopRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        _troopRect.Size = _iconSize * .7f * Vector2.One;
        _troopRect.Position = -_troopRect.Size / 2f;
        AddChild(_troopRect);
        
        var healthMarkerPos = new Vector2(_iconSize * .35f, -_iconSize * .35f);

        _healthMesh = new MeshInstance2D();
        _healthMesh.Position = new Vector2(_iconSize * .3f, -_iconSize * .3f);
        _healthMesh.Mesh = _health;
        AddChild(_healthMesh);
        
        Draw(unit, data);
    }
    public void Draw(Unit unit, Data data)
    {
        var health = unit.GetHealth(data);
        var healthRatio = Mathf.Clamp(health.X / health.Y, 0f, 1f);
        _healthMesh.Modulate = Colors.Red.Lerp(Colors.Green, healthRatio);

        _regimeColor.Modulate = unit.Regime.Get(data).GetUnitColor();
        
        if (unit.GetGroup(data) is UnitGroup g)
        {
            _groupColor.Modulate = g.Color;
        }
        else
        {
            _groupColor.Modulate = Colors.White;
        }
        
        var maxPowerTroop = unit.GetMaxPowerTroop(data);

        _troopRect.Texture = maxPowerTroop.Icon.Texture;
        // _powerPoints.Text = Mathf.RoundToInt(totalPp).ToString();
    }
}