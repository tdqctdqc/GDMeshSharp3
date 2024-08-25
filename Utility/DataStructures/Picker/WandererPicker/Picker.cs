using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Picker<T>
{
    public HashSet<T> NotTaken { get; private set; }
    public HashSet<IPickerAgent<T>> OpenPickers { get; private set; }
    public List<IPickerAgent<T>> Agents { get; private set; }
    public Func<T, IEnumerable<T>> GetNeighbors { get; private set; }
    public Picker(IEnumerable<T> notTaken,
        Func<T, IEnumerable<T>> getNeighbors)
    {
        GetNeighbors = getNeighbors;
        NotTaken = notTaken.ToHashSet();
        OpenPickers = new HashSet<IPickerAgent<T>>();
        Agents = new List<IPickerAgent<T>>();
    }

    public void AddAgent(IPickerAgent<T> w)
    {
        NotTaken.RemoveWhere(w.Picked.Contains);
        OpenPickers.Add(w);
        Agents.Add(w);
    }

    
    public void RandomAgentPick(int toLeave = 0)
    {
        while (OpenPickers.Count > 0 && NotTaken.Count > toLeave)
        {
            var wanderer = OpenPickers.GetRandomElement();
            var open = wanderer.Pick(this);
            if (open == false) OpenPickers.Remove(wanderer);
        }
    }
    
    
}
