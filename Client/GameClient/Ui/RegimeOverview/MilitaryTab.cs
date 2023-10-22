using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class MilitaryTab : ScrollContainer
{
    private VBoxContainer _container;
    public MilitaryTab()
    {
        Name = "Military";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        _container.CreateLabelAsChild("TROOP RESERVE");
        var tick = client.Data.BaseDomain.GameClock.Tick;
        
        foreach (var kvp in regime.Military.TroopReserve.GetEnumerableModel(client.Data))
        {
            var amt = kvp.Value;
            var troop = kvp.Key;
            var hbox = new HBoxContainer();
            hbox.AddChild(troop.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"Amount: {amt} ");
            _container.AddChild(hbox);
        }
        var units = regime.GetUnits(client.Data);
        if (units == null) return;

        _container.CreateLabelAsChild("TROOPS DEPLOYED");
        var troopsDeployed = new Dictionary<Troop, float>();
        foreach (var unit in units)
        {
            foreach (var kvp in unit.Troops.GetEnumerableModel(client.Data))
            {
                troopsDeployed.AddOrSum(kvp.Key, kvp.Value);
            }
        }        
        foreach (var kvp in troopsDeployed)
        {
            var amt = kvp.Value;
            var troop = kvp.Key;
            var hbox = new HBoxContainer();
            hbox.AddChild(troop.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"Amount: {amt} ");
            _container.AddChild(hbox);
        }
        _container.CreateLabelAsChild("UNITS");
        _container.CreateLabelAsChild($"{units.Count()} Units");
        foreach (var unit in units)
        {
            _container.CreateLabelAsChild(unit.Template.Entity(client.Data).Name);
        }
    }
}