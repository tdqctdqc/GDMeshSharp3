using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Frontline : Entity
{
    public ERef<Alliance> Alliance { get; private set; }
    public List<FrontFace> Faces { get; private set; }
    public HashSet<CellRef> AdvanceInto { get; private set; }
    public static float DefMultForNotAtWarCell { get; private set; } 
        = .5f;
    public static Frontline Create(List<FrontFace> faces, 
        HashSet<CellRef> advanceInto,
        Alliance alliance, ICreateKey key)
    {
        var f = new Frontline(key.GetData().IdDispenser.TakeId(),
            alliance.MakeRef(),
            faces, advanceInto);
        key.Create(f);
        return f;
    }
    
    public Frontline(int id, ERef<Alliance> alliance, 
        List<FrontFace> faces, HashSet<CellRef> advanceInto)
            : base(id)
    {
        Alliance = alliance;
        Faces = faces;
        AdvanceInto = advanceInto;
    }
    
    public HashSet<Cell> GetRivalOpposingCells(Data data)
    {
        return Faces
            .Select(f => f.GetForeign(data))
            .Distinct()
            .Where(f => f.Controller.Get(data)
                .GetAlliance(data).IsRivals(Alliance.Get(data), data))
            .ToHashSet();
    }
    public float GetOpposingPowerPointsWeighted(Data data)
    {
        return Faces.Select(f => f.GetNative(data))
            .Distinct()
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(Alliance.Get(data), data))
            .Sum(c => data.Context.PowerPoints[c]);
    }

    public void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        foreach (var face in Faces)
        {
            mb.DrawCellBorder(
                face.GetNative(d), face.GetForeign(d),
                c => Colors.Black, 10f, relTo, d);
            mb.DrawCellBorder(
                face.GetNative(d), face.GetForeign(d),
                c => Colors.White, 5f, relTo, d);
        }
    }
    public override void CleanUp(IWriteKey key)
    {
        
    }
}