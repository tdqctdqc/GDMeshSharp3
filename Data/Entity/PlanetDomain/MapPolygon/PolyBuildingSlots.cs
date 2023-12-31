using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolyBuildingSlots
{
    public Dictionary<BuildingType, LinkedList<PolyTriPosition>> AvailableSlots { get; private set; }
    public int this[BuildingType type] => AvailableSlots.ContainsKey(type) ? AvailableSlots[type].Count : 0;

    public static PolyBuildingSlots Construct()
    {
        return new PolyBuildingSlots(new Dictionary<BuildingType, LinkedList<PolyTriPosition>>());
    }
    [SerializationConstructor] private PolyBuildingSlots(Dictionary<BuildingType, LinkedList<PolyTriPosition>> availableSlots)
    {
        AvailableSlots = availableSlots;
    }
    
    public void RemoveSlot(BuildingType type, PolyTriPosition pos)
    {
        var removed = AvailableSlots[type].Remove(pos);
        if (removed == false) throw new Exception();
    }
    public void SetSlotNumbers(MapPolygon poly, StrongWriteKey key)
    {
        var industrySlots = 5;
        var govSlots = 1;
        var extractSlots = 5;
        var infraSlots = 5;
        var financialSlots = 3;
        var milSlots = 1;
        var totalSlots = industrySlots + govSlots + extractSlots + financialSlots + milSlots;
        
        AvailableSlots.Clear();
        var tris = poly.Tris.Tris.Where(t => t.Landform(key.Data).IsLand)
            .Select(t => t.Index)
            .OrderBy(t => Game.I.Random.Randi())
            .ToHashSet();
        
        if (totalSlots > tris.Count)
        {
            return;
            throw new Exception($"{totalSlots} slots {tris.Count} tris");
        }
        
        AddSlots(BuildingType.Industry, poly, tris, industrySlots);
        AddSlots(BuildingType.Government, poly, tris, govSlots);
        AddSlots(BuildingType.Extraction, poly, tris, extractSlots);
        AddSlots(BuildingType.Financial, poly, tris, financialSlots);
        AddSlots(BuildingType.Military, poly, tris, milSlots);
    }
    private void AddSlots(BuildingType type, MapPolygon poly, HashSet<byte> availTriIds, int num)
    {
        AvailableSlots.Add(type, new LinkedList<PolyTriPosition>());
        for (var i = 0; i < num; i++)
        {
            if (availTriIds.Count == 0) throw new Exception();
            var id = availTriIds.First();
            availTriIds.Remove(id);
            AvailableSlots[type].AddLast(new PolyTriPosition(poly.Id, id));
        }
    }
}
