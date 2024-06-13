using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class MilitaryTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public MilitaryTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Military";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }

    private MilitaryTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
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
            .GetAll<Army>()
            .Where(g => g.Regime.RefId == regime.Id);
        _container.CreateLabelAsChild($"{units.Count()} Units");

        if (groups.Any())
        {
            _container.CreateLabelAsChild($"{groups.Count()} Groups");
            _container.CreateLabelAsChild("GROUPS");
            foreach (var group in groups)
            {
                _container.CreateLabelAsChild("Group " + group.Id);
                var gUnits = group.Units.Entities(client.Data);
                foreach (var unit in gUnits)
                {
                    _container.CreateLabelAsChild("\t" + unit.Template.Get(client.Data).Name);
                }
            }
        }
    }
}