using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Picker<T>
{
    public HashSet<T> NotTaken { get; private set; }
    public HashSet<PickerAgent<T>> OpenPickers { get; private set; }
    public List<PickerAgent<T>> Agents { get; private set; }
    public Func<T, IEnumerable<T>> GetNeighbors { get; private set; }
    public Picker(IEnumerable<T> notTaken,
        Func<T, IEnumerable<T>> getNeighbors)
    {
        GetNeighbors = getNeighbors;
        NotTaken = notTaken.ToHashSet();
        OpenPickers = new HashSet<PickerAgent<T>>();
        Agents = new List<PickerAgent<T>>();
    }

    public void AddAgent(PickerAgent<T> w)
    {
        OpenPickers.Add(w);
        Agents.Add(w);
    }

    public void Pick(Data data)
    {
        while (OpenPickers.Count > 0 && NotTaken.Count > 0)
        {
            var wanderer = OpenPickers.GetRandomElement();
            var open = wanderer.Pick(this, data);
            if (open == false) OpenPickers.Remove(wanderer);
        }
    }
}
