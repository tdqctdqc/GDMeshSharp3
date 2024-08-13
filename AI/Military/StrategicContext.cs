
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class StrategicContext
{
    
    public HashSet<Cell> AlliedCells { get; private set; }
    public List<HashSet<Cell>> Unions { get; private set; }
    public HashSet<Cell> Gained { get; private set; }
    public List<HashSet<Cell>> GainedUnions { get; private set; }
    public Graph<Vector3I, HashSet<List<FrontFace>>> Graph { get; private set; }
    public HashSet<Cell> Lost { get; private set; }
    public List<HashSet<Cell>> LostUnions { get; private set; }
    public HashSet<Cell> Stable { get; private set; }
    public List<HashSet<Cell>> StableUnions { get; private set; }

    public Dictionary<List<FrontFace>, HashSet<List<FrontFace>>> EdgeMergeMap { get; private set; }
    public Dictionary<List<FrontFace>, ERef<Frontline>> ValidEdgesFrontlines { get; private set; }
    public StrategicContext(Alliance alliance, 
        HashSet<Cell> prev,
        Data d)
    {
        Graph = new Graph<Vector3I, HashSet<List<FrontFace>>>();
        var alliedCells = d.Planet.MapAux
            .CellHolder.Cells.Values
            .Where(c => c.FriendlyControlled(alliance, d))
            .ToArray();
        AlliedCells = alliedCells
            .ToHashSet();
        Unions = UnionFind.Find<Cell, HashSet<Cell>>(alliedCells,
            (p, q) => true,
            p => p.GetNeighbors(d))
            .ToList();
        
        
        Gained = AlliedCells.Except(prev).ToHashSet();
        if (prev.Count == 0)
        {
            Gained = new HashSet<Cell>();
        }
        GainedUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            Gained, (c, d) => true, 
            c => c.GetNeighbors(d));
        
        
        Lost = prev.Except(AlliedCells).ToHashSet();
        if (prev.Count == 0)
        {
            Lost = new HashSet<Cell>();
        }
        LostUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            Lost, (c, d) => true, 
            c => c.GetNeighbors(d));

        Stable = AlliedCells.Intersect(prev).ToHashSet();
        if (prev.Count == 0)
        {
            Stable = AlliedCells.ToHashSet();
        }
        StableUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            Stable, (c, d) => true, c => c.GetNeighbors(d));

        EdgeMergeMap = new Dictionary<List<FrontFace>, HashSet<List<FrontFace>>>();
        ValidEdgesFrontlines = new Dictionary<List<FrontFace>, ERef<Frontline>>();
    }
    public CellChangeStatus GetCellChangeStatus(Cell c)
    {
        if (Stable.Contains(c)) return CellChangeStatus.Stable;
        if (Gained.Contains(c)) return CellChangeStatus.Gained;
        if (Lost.Contains(c)) return CellChangeStatus.Lost;
        return CellChangeStatus.Out;
    }
    
    
    public Color GetFaceColor(FrontFace face, Data d)
    {
        var inside = GetCellChangeStatus(face.GetNative(d));
        var outside = GetCellChangeStatus(face.GetForeign(d));
        if (inside == CellChangeStatus.Stable)
        {
            if (outside == CellChangeStatus.Gained)
            {
                return Colors.Yellow;
            }
            else if (outside == CellChangeStatus.Lost)
            {
                return Colors.Purple;
            }
            else if (outside == CellChangeStatus.Out)
            {
                return Colors.Orange;
            }
            else
            {
                throw new Exception();
            }
        }
        else if (inside == CellChangeStatus.Gained)
        {
            if (outside == CellChangeStatus.Lost)
            {
                return Colors.Purple;
            }
            else if (outside == CellChangeStatus.Out)
            {
                return Colors.Purple;
            }
            else
            {
                throw new Exception();
            }      
        }
        else if (inside == CellChangeStatus.Lost)
        {
            if (outside == CellChangeStatus.Out)
            {
                return Colors.Transparent;
            }
            else
            {
                throw new Exception();
            }   
        }
        else throw new Exception();
    }
    
    
    public void DrawCells(MeshBuilder mb, Vector2 relTo, Data d)
    {
        int i = 0;
        foreach (var union in StableUnions)
        {
            var color = Colors.Blue.GetPeriodicShade(i++);
            foreach (var cell in union)
            {
                mb.DrawCellRel(cell, relTo, color, d);
            }
        }
        foreach (var union in GainedUnions)
        {
            var color = Colors.Green.GetPeriodicShade(i++);
            foreach (var cell in union)
            {
                mb.DrawCellRel(cell, relTo, color, d);
            }
        }
        foreach (var union in LostUnions)
        {
            var color = Colors.Red.GetPeriodicShade(i++);
            foreach (var cell in union)
            {
                mb.DrawCellRel(cell, relTo, color, d);
            }
        }
    }

}