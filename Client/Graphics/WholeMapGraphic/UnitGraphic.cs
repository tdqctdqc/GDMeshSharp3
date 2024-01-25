
using System.Linq;
using Godot;

public partial class UnitGraphic : Node2D
{
    private MeshInstance2D 
        _borderAndGroupColor,
        _regimeColor,
        _healthMesh;

    private Label _powerPoints;
    private TextureRect _troopRect;
    private static LabelSettings _labelSettings;

    static UnitGraphic()
    {
        _labelSettings = new LabelSettings();
        _labelSettings.FontSize = 20;
    }
    private UnitGraphic() {}
    
    public UnitGraphic(Unit unit, Data data)
    {
        var mb = new MeshBuilder();
        var regime = unit.Regime.Entity(data);
        var iconSize = 15f;
        mb.AddPoint(Vector2.Zero, iconSize, Colors.Black);
        mb.AddPoint(Vector2.Zero, iconSize - .2f, Colors.White);
        _borderAndGroupColor = mb.GetMeshInstance();
        AddChild(_borderAndGroupColor);
        mb.Clear();
        
        mb.AddPoint(Vector2.Zero, iconSize * .8f, regime.GetUnitColor());
        _regimeColor = mb.GetMeshInstance();
        AddChild(_regimeColor);
        mb.Clear();
        
        _troopRect = new TextureRect();
        _troopRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        _troopRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        _troopRect.Size = iconSize * .7f * Vector2.One;
        _troopRect.Position = -_troopRect.Size / 2f;
        AddChild(_troopRect);
        
        var healthMarkerSize = iconSize * .1f;
        var healthMarkerPos = new Vector2(iconSize * .35f, -iconSize * .35f);
        mb.AddPoint(healthMarkerPos, healthMarkerSize,
            Colors.Black);
        mb.AddPoint(healthMarkerPos, healthMarkerSize * .8f,
            Colors.White);
        _healthMesh = mb.GetMeshInstance();
        AddChild(_healthMesh);
        mb.Clear();

        _powerPoints = new Label();
        _powerPoints.LabelSettings = _labelSettings;
        _powerPoints.Scale = .1f * Vector2.One;
        _powerPoints.HorizontalAlignment = HorizontalAlignment.Center;
        _powerPoints.Position = new Vector2(0f, iconSize * .2f);
        AddChild(_powerPoints);
        
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

        if (unit.GetGroup(data) is UnitGroup g)
        {
            _borderAndGroupColor.Modulate = g.Color;
        }
        else
        {
            _borderAndGroupColor.Modulate = Colors.White;
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
        _powerPoints.Text = Mathf.RoundToInt(totalPp).ToString();
    }
}