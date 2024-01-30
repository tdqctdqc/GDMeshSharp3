using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyTooltipTemplate : TooltipTemplate<(MapPolygon poly, PolyCell cell)>
{
    public PolyTooltipTemplate() : base()
    {
    }

    protected override List<Func<(MapPolygon poly, PolyCell cell), Data, Control>> _fastGetters { get; }
        = new List<Func<(MapPolygon poly, PolyCell cell), Data, Control>>
        {
            GetId,
            GetRegime,
            GetLandform,
            GetVeg,
        };
    protected override List<Func<(MapPolygon poly, PolyCell cell), Data, Control>> _slowGetters { get; }
        = new List<Func<(MapPolygon poly, PolyCell cell), Data, Control>>
        {
            GetSettlementName,
            // GetSettlementSize, 
            GetPeeps,
            GetBuildings,
            GetConstructions,
            GetResourceDeposits,
            // GetAltitude,
            // GetSlots
        };

    private static Control GetBuildings((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var bs = t.poly.GetBuildings(d);
        var control = new VBoxContainer();
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;
        if (bs != null)
        {
            var counts = bs
                .Select(b => b.Model.Model(d)).GetCounts();
            foreach (var kvp in counts)
            {
                var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                    kvp.Key.Icon, kvp.Value.ToString(), iconSize);
                control.AddChild(box);
            }
        }

        var foods = t.poly.PolyFoodProd.Nums;
        foreach (var kvp in foods)
        {
            var technique = d.Models.GetModel<FoodProdTechnique>(kvp.Key);
            var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                technique.Icon, kvp.Value.ToString(), iconSize);
            control.AddChild(box);
        }
        return control;
    }

    private static Control GetAltitude((MapPolygon poly, PolyCell cell) t, Data d)
    {
        return NodeExt.CreateLabel("Altitude: " + t.poly.Altitude);
    }

    private static Control GetVeg((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var tri = t.cell;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Landform: " + tri.GetVegetation(d).Name);
    }

    private static Control GetLandform((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var tri = t.cell;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Landform: " + tri.GetLandform(d).Name);
    }

    private static Control GetRegime((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var polyR = t.poly.OwnerRegime;
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;

        if (polyR.IsEmpty())
        {
            return NodeExt.CreateLabel("None");
        }

        var r = polyR.Entity(d);
        var box = NodeExt.GetLabeledIcon<HBoxContainer>(
            polyR.Entity(d).Template.Model(d).Flag,
            r.Name, iconSize);
        return box;
    }
    private static Control GetPeeps((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var peeps = t.poly.GetPeep(d);
        if (peeps == null)
        {
            var l = new Label();
            l.Text = "No Peeps";
            return l;
        }
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;

        var jobs = new VBoxContainer();
        var size = new Label();
        size.Text = "Num Peeps: " + peeps.Size;
        jobs.AddChild(size);
        var peepJobCounts = t.poly.GetPeep(d).Employment.Counts
            // .Where(kvp => kvp.Value > 0)
            .Select(kvp => new KeyValuePair<PeepJob, int>((PeepJob)d.Models[kvp.Key], kvp.Value))
            .ToList();
        foreach (var peepJobCount in peepJobCounts)
        {
            var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                peepJobCount.Key.Icon, peepJobCount.Value.ToString(), 
                iconSize);
            jobs.AddChild(box);
        }
        return jobs;
    }
    private static Control GetConstructions((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var entries = new VBoxContainer();
        if (d.Infrastructure.CurrentConstruction.ByPoly.ContainsKey(t.poly.Id) == false)
            return entries;
        var constructions = d.Infrastructure.CurrentConstruction.ByPoly[t.poly.Id];
        if(constructions.Count == 0) 
            return entries;
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;

        var kvps = constructions.Select(
            c => new KeyValuePair<BuildingModel, Vector2>
            (c.Model.Model(d), 
                new Vector2(c.Model.Model(d).NumTicksToBuild - c.TicksLeft, c.Model.Model(d).NumTicksToBuild))
        );
        
        foreach (var kvp in kvps)
        {
            var progress = kvp.Value;
            var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                kvp.Key.Icon, $"{(int)progress.X} / {(int)progress.Y}",
                iconSize);
            entries.AddChild(box);
        }

        return entries;
    }
    private static Control GetId((MapPolygon poly, PolyCell cell) t, Data d)
    {
        return NodeExt.CreateLabel("Poly Id: " + t.poly.Id.ToString());
    }
    private static Control GetResourceDeposits((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var iconSize = Vector2.One * Game.I.Client.Settings.MedIconSize.Value;

        var rs = t.poly.GetResourceDeposits(d);
        if (rs != null)
        {
            var label = new Label();
            int iter = 0;
            foreach (var r in rs)
            {
                if (iter != 0) label.Text += "\n";
                label.Text += $"{r.Item.Model(d).Name}: {Mathf.FloorToInt(r.Size)}";
            }

            return label;
        }
        return null;
    }

    private static Control GetSettlementSize((MapPolygon poly, PolyCell cell) t, Data d)
    {
        return  d.Infrastructure.SettlementAux.ByPoly[t.poly] is Settlement s
            ? NodeExt.CreateLabel("Settlement Size: " + t.poly.GetPeep(d).Size)
            : null;
    }

    private static Control GetSettlementName((MapPolygon poly, PolyCell cell) t, Data d)
    {
        return  d.Infrastructure.SettlementAux.ByPoly[t.poly] is Settlement s
            ? NodeExt.CreateLabel("Settlement Name: " + s.Name)
            : null;
    }

    private static Control GetSlots((MapPolygon poly, PolyCell cell) t, Data d)
    {
        var c = new VBoxContainer();
        foreach (var kvp in t.poly.PolyBuildingSlots.AvailableSlots)
        {
            c.AddChild(NodeExt.CreateLabel($"Available {kvp.Key} Slots: {kvp.Value.Count}"));
        }
        return c;
    }
}
