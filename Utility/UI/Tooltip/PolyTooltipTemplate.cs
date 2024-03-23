using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyTooltipTemplate : TooltipTemplate<(MapPolygon poly, Cell cell)>
{
    public PolyTooltipTemplate() : base()
    {
    }

    protected override List<Func<(MapPolygon poly, Cell cell), Data, Control>> _fastGetters { get; }
        = new List<Func<(MapPolygon poly, Cell cell), Data, Control>>
        {
            GetId,
            GetRegime,
            GetLandform,
            GetVeg,
            GetPop
        };
    protected override List<Func<(MapPolygon poly, Cell cell), Data, Control>> _slowGetters { get; }
        = new List<Func<(MapPolygon poly, Cell cell), Data, Control>>
        {
            GetSettlementName,
            // GetSettlementSize, 
            GetPeeps,
            GetBuildings,
            GetFoodProd,
            GetResourceDeposits,
            // GetAltitude,
            // GetSlots
        };

    private static Control GetPop((MapPolygon poly, Cell cell) t, Data d)
    {
        var s = "";
        var cells = t.poly.GetCells(d).Where(c => c.HasPeep(d));
        if (cells.Count() > 0)
        {
            s += "Poly pop: " 
                 + cells.Sum(c => c.GetPeep(d).Size)
                 + "\n";
        }

        if (t.cell is LandCell)
        {
            s += "Cell pop: " + t.cell.GetPeep(d).Size;
        }

        return NodeExt.CreateLabel(s);
    }
    private static Control GetBuildings((MapPolygon poly, Cell cell) t, Data d)
    {
        var bs = t.poly.GetBuildings(d);
        var control = new VBoxContainer();
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;
        if (bs != null)
        {
            var counts = bs
                .Select(b => b.Model.Get(d)).GetCounts();
            foreach (var kvp in counts)
            {
                var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                    kvp.Key.Icon, kvp.Value.ToString(), iconSize);
                control.AddChild(box);
            }
        }

        
        return control;
    }
    private static Control GetFoodProd((MapPolygon poly, Cell cell) t, Data d)
    {
        if (t.cell is LandCell l == false) return new Control();
        var polyFoodCounts = t.poly
            .GetCells(d).OfType<LandCell>()
            .Select(c => c.FoodProd.Nums)
            .MergeCounts();
        
        var bs = t.cell;
        var control = new VBoxContainer();
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;
        
        foreach (var (model, num) in l.FoodProd.Nums)
        {
            var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                model.Get(d).Icon, 
                $"{num.RoundTo2Digits()} " +
                $"/ {polyFoodCounts[model].RoundTo2Digits()}",
                iconSize);
            control.AddChild(box);
        }
        
        return control;
    }
    private static Control GetAltitude((MapPolygon poly, Cell cell) t, Data d)
    {
        return NodeExt.CreateLabel("Altitude: " + t.poly.Altitude);
    }

    private static Control GetVeg((MapPolygon poly, Cell cell) t, Data d)
    {
        var tri = t.cell;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Landform: " + tri.GetVegetation(d).Name);
    }

    private static Control GetLandform((MapPolygon poly, Cell cell) t, Data d)
    {
        var tri = t.cell;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Landform: " + tri.GetLandform(d).Name);
    }

    private static Control GetRegime((MapPolygon poly, Cell cell) t, Data d)
    {
        var polyR = t.cell.Controller;
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;

        if (polyR.IsEmpty())
        {
            return NodeExt.CreateLabel("None");
        }

        var r = polyR.Get(d);
        var box = NodeExt.GetLabeledIcon<HBoxContainer>(
            polyR.Get(d).Template.Get(d).Flag,
            r.Name, iconSize);
        return box;
    }
    private static Control GetPeeps((MapPolygon poly, Cell cell) t, Data d)
    {
        return new Control();
    }
    
    private static Control GetId((MapPolygon poly, Cell cell) t, Data d)
    {
        return NodeExt.CreateLabel("Poly Id: " + t.poly.Id.ToString());
    }
    private static Control GetResourceDeposits((MapPolygon poly, Cell cell) t, Data d)
    {
        var iconSize = Vector2.One * Game.I.Client.Settings.MedIconSize.Value;

        var rs = t.poly
            .GetResourceDeposits(d)
            .GetCountsBy(rd => rd.Item.Get(d));
        if (rs != null)
        {
            var label = new Label();
            int iter = 0;
            foreach (var (item, amt) in rs)
            {
                if (iter != 0) label.Text += "\n";
                label.Text += $"{item.Name}: {amt}";
            }

            return label;
        }
        return null;
    }


    private static Control GetSettlementName((MapPolygon poly, Cell cell) t, Data d)
    {
        return  d.Infrastructure.SettlementAux.ByCell[t.cell] is Settlement s
            ? NodeExt.CreateLabel("Settlement Name: " + s.Name)
            : null;
    }
}
