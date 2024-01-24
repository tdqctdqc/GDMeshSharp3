
using System.Linq;
using Godot;

public partial class UnitGraphic : Node2D
{
    private UnitGraphic() {}
    
    public UnitGraphic(Unit unit, Data data)
    {
        Draw(unit, data);
    }
    public void Draw(Unit unit, Data data)
    {
        this.ClearChildren();
        var mb = new MeshBuilder();
        var regime = unit.Regime.Entity(data);
        var group = unit.GetGroup(data);
        var groupColor = group != null ? group.Color : Colors.White;
        var iconSize = 15f;
        mb.AddPoint(Vector2.Zero, iconSize, Colors.Black);
        mb.AddPoint(Vector2.Zero, iconSize - .2f, groupColor);
        mb.AddPoint(Vector2.Zero, iconSize * .8f, regime.GetUnitColor());
        
        
        
        
        var maxPowerId = unit.Troops.Contents
            .MaxBy(kvp =>
            {
                var unit = data.Models.GetModel<Troop>(kvp.Key);
                var power = kvp.Value * unit.GetPowerPoints();
                return power;
            }).Key;
        var maxPowerTroop = data.Models.GetModel<Troop>(maxPowerId);
        
        var totalPp = unit.Troops.GetEnumerableModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        var templatePp = unit.Template.Entity(data).TroopCounts.GetEnumerableModel(data)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);


        var ratio = Mathf.Clamp(totalPp / templatePp, 0f, 1f);
        var healthMarkerSize = iconSize * .1f;
        var healthMarkerPos = Vector2.One * iconSize * .25f;
        mb.AddPoint(healthMarkerPos, healthMarkerSize,
            Colors.Black);
        mb.AddPoint(healthMarkerPos, healthMarkerSize * .8f,
            Colors.Red.Lerp(Colors.Green, ratio));
        
        AddChild(mb.GetMeshInstance());
        var troopPic = maxPowerTroop.Icon
            .GetMeshInstance(iconSize * .7f);
        AddChild(troopPic);
    }
}