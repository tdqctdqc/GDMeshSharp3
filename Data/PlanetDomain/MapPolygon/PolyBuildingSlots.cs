using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolyBuildingSlots
{
    public Dictionary<BuildingType, LinkedList<int>> AvailableSlots { get; private set; }
    public int this[BuildingType type] => AvailableSlots.ContainsKey(type) ? AvailableSlots[type].Count : 0;

    public static PolyBuildingSlots Construct()
    {
        return new PolyBuildingSlots(new Dictionary<BuildingType, LinkedList<int>>());
    }
    [SerializationConstructor] private PolyBuildingSlots(Dictionary<BuildingType, LinkedList<int>> availableSlots)
    {
        AvailableSlots = availableSlots;
    }
    
    public void RemoveSlot(BuildingType type, int pos)
    {
        var removed = AvailableSlots[type].Remove(pos);
        if (removed == false) throw new Exception();
    }
    public void SetSlotNumbers(MapPolygon poly, StrongWriteKey key)
    {
        if (poly.IsWater()) return;
        var industrySlots = 5;
        var govSlots = 1;
        var extractSlots = 5;
        var infraSlots = 5;
        var financialSlots = 3;
        var milSlots = 1;
        var totalSlots = industrySlots + govSlots + extractSlots + financialSlots + milSlots;
        
        AvailableSlots.Clear();
        var cells = poly.GetCells(key.Data)
            .Where(c => c is IPolyCell)
            .OrderBy(t => Game.I.Random.Randi())
            .Select(c => c.Id)
            .ToHashSet();
        
        if (totalSlots > cells.Count)
        {
            return;
            throw new Exception($"{totalSlots} slots {cells.Count} tris");
        }
        
        AddSlots(BuildingType.Industry, poly, cells, industrySlots);
        AddSlots(BuildingType.Government, poly, cells, govSlots);
        AddSlots(BuildingType.Extraction, poly, cells, extractSlots);
        AddSlots(BuildingType.Financial, poly, cells, financialSlots);
        AddSlots(BuildingType.Military, poly, cells, milSlots);
    }
    private void AddSlots(BuildingType type, 
        MapPolygon poly, HashSet<int> availCellIds, int num)
    {
        AvailableSlots.Add(type, new LinkedList<int>());
        for (var i = 0; i < num; i++)
        {
            if (availCellIds.Count == 0) throw new Exception();
            var id = availCellIds.First();
            availCellIds.Remove(id);
            AvailableSlots[type].AddLast(id);
        }
    }
}
