using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WandererPicker
{
    public HashSet<MapPolygon> NotTaken { get; private set; }
    public HashSet<Wanderer> OpenPickers { get; private set; }
    public List<Wanderer> Wanderers { get; private set; }

    public WandererPicker(IEnumerable<MapPolygon> notTaken)
    {
        NotTaken = notTaken.ToHashSet();
        OpenPickers = new HashSet<Wanderer>();
        Wanderers = new List<Wanderer>();
    }

    public void AddWanderer(Wanderer w)
    {
        OpenPickers.Add(w);
        Wanderers.Add(w);
    }

    public void Pick(Data data)
    {
        while (OpenPickers.Count > 0 && NotTaken.Count > 0)
        {
            var wanderer = OpenPickers.GetRandomElement();
            var open = wanderer.MoveAndPick(this, data);
            if (open == false) OpenPickers.Remove(wanderer);
        }
    }
}
