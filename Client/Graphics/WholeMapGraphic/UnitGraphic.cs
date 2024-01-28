
using System.Linq;
using Godot;

public partial class UnitGraphic : Node2D
{
    private MeshInstance2D 
        _borderColor,
        _groupColor,
        _regimeColor,
        _healthMesh;

    // private Label _powerPoints;
    private TextureRect _troopRect, _flagRect;
    private static LabelSettings _labelSettings;
    private static QuadMesh _border, _group, _regime, 
        _health;

    private static float _iconSize = 15f;
    static UnitGraphic()
    {
        _labelSettings = new LabelSettings();
        _labelSettings.FontSize = 25;
        _border = new QuadMesh();
        _border.Size = Vector2.One * _iconSize;
        _group = new QuadMesh();
        _group.Size = Vector2.One * (_iconSize - .2f);
        _regime = new QuadMesh();
        _regime.Size = Vector2.One * _iconSize * .8f;
        _health = new QuadMesh();
        _health.Size = Vector2.One * _iconSize * .15f;
    }
    private UnitGraphic() {}
    
    public UnitGraphic(Unit unit, Data data)
    {
        var regime = unit.Regime.Entity(data);
        
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

        _flagRect = regime.Template.Model(data).Flag.
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

        // _powerPoints = new Label();
        // _powerPoints.LabelSettings = _labelSettings;
        // _powerPoints.Scale = .1f * Vector2.One;
        // _powerPoints.HorizontalAlignment = HorizontalAlignment.Center;
        // _powerPoints.AnchorsPreset = (int)Control.LayoutPreset.CenterTop;
        // _powerPoints.Position = new Vector2(-_iconSize * .25f, -_iconSize * .4f);
        // AddChild(_powerPoints);
        
        Draw(unit, data);
    }
    public void Draw(Unit unit, Data data)
    {
        var totalPp = unit.Troops.GetEnumerableModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        var templatePp = unit.Template.Entity(data).TroopCounts.GetEnumerableModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        var healthRatio = Mathf.Clamp(totalPp / templatePp, 0f, 1f);
        _healthMesh.Modulate = Colors.Red.Lerp(Colors.Green, healthRatio);

        _regimeColor.Modulate = unit.Regime.Entity(data).GetUnitColor();
        
        if (unit.GetGroup(data) is UnitGroup g)
        {
            _groupColor.Modulate = g.Color;
        }
        else
        {
            _groupColor.Modulate = Colors.White;
        }
        
        
        var maxPowerId = unit.Troops.Contents
            .MaxBy(kvp =>
            {
                var unit = data.Models.GetModel<Troop>(kvp.Key);
                var power = kvp.Value * unit.GetPowerPoints();
                return power;
            }).Key;
        var maxPowerTroop = data.Models.GetModel<Troop>(maxPowerId);

        _troopRect.Texture = maxPowerTroop.Icon.Texture;
        // _powerPoints.Text = Mathf.RoundToInt(totalPp).ToString();
    }
}