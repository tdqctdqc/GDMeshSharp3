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
        var iconSize = client.Settings.MedIconSize.Value;

        foreach (var kvp in regime.Stock.Stock
                     .GetEnumerableModel(client.Data)
                     .Where(k => k.Key is Troop t))
        {
            var amt = kvp.Value;
            var troop = (Troop)kvp.Key;
            var hbox = troop.Icon.GetLabeledIcon<HBoxContainer>(
                amt.ToString(), iconSize);
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
            var hbox = troop.Icon.GetLabeledIcon<HBoxContainer>(
                amt.ToString(), iconSize);
            _container.AddChild(hbox);
        }

        var groups = client.Data
            .GetAll<UnitGroup>()
            .Where(g => g.Regime.RefId == regime.Id);
        _container.CreateLabelAsChild($"{units.Count()} Units");

        if (groups.Count() > 0)
        {
            _container.CreateLabelAsChild($"{groups.Count()} Groups");
            _container.CreateLabelAsChild("GROUPS");
            foreach (var group in groups)
            {
                _container.CreateLabelAsChild("Group " + group.Id);
                var gUnits = group.Units.Items(client.Data);
                foreach (var unit in gUnits)
                {
                    _container.CreateLabelAsChild("\t" + unit.Template.Get(client.Data).Name);
                }
            }
        }
    }
}