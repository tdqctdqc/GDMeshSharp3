using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Frontline
{
    public Alliance Alliance { get; private set; }
    public List<FrontFace> Faces { get; private set; }
    public HashSet<Cell> AdvanceInto { get; private set; }
    public Dictionary<FrontFace, float> FaceAttackWeights { get; private set; }
    public Dictionary<FrontFace, float> FaceDefendWeights { get; private set; }
    public float AttackWeight { get; private set; }
    public float DefendWeight { get; private set; }
    public static float DefMultForNotAtWarCell { get; private set; } 
        = .5f;
    public Frontline(List<FrontFace> faces, 
        HashSet<Cell> advanceInto,
        Alliance alliance)
    {
        Faces = faces;
        Alliance = alliance;
        AdvanceInto = advanceInto;
        AttackWeight = 0f;
        FaceAttackWeights = new Dictionary<FrontFace, float>();
        DefendWeight = 0f;
        FaceDefendWeights = new Dictionary<FrontFace, float>();
    }

    public void AddAttackWeight(float w,
        IEnumerable<Cell> attack)
    {
        AttackWeight += w;
        var ids = attack.Select(a => a.Id).ToHashSet();
        for (var i = 0; i < Faces.Count; i++)
        {
            var face = Faces[i];
            if (ids.Contains(face.Foreign))
            {
                FaceAttackWeights.AddOrSum(face, w);
            }
        }

        AdvanceInto.UnionWith(attack);
    }
    
    public void AddAttackWeight(float w,
        Cell attack)
    {
        AttackWeight += w;
        for (var i = 0; i < Faces.Count; i++)
        {
            var face = Faces[i];
            if (face.Foreign == attack.Id)
            {
                FaceAttackWeights.AddOrSum(face, w);
            }
        }

        AdvanceInto.Add(attack);
    }
    
    public void AddDefendWeightAlongWholeLine(float w, Data d)
    {
        DefendWeight += w;
        var totalCellDef = Faces.Sum(
            f => 1f / ((LandCell)f.GetNative(d)).GetLandDefendScore(d));
        
        for (var i = 0; i < Faces.Count; i++)
        {
            var face = Faces[i];
            var native = (LandCell)face.GetNative(d);
            var foreign = face.GetForeign(d);
            var mult = foreign.Controller.Get(d)
                .GetAlliance(d).IsAtWar(Alliance, d)
                ? 1f : DefMultForNotAtWarCell;
            if (totalCellDef == 0f)
            {
                FaceDefendWeights.AddOrSum(face, w / Faces.Count);
            }
            else
            {
                var cellDefRatio = (1f / native.GetLandDefendScore(d)) / totalCellDef;
                FaceDefendWeights.AddOrSum(face, cellDefRatio * w);
            }
        }
    }
    public HashSet<Cell> GetRivalOpposingCells(Data data)
    {
        return Faces
            .Select(f => f.GetForeign(data))
            .Distinct()
            .Where(f => f.Controller.Get(data)
                .GetAlliance(data).IsRivals(Alliance, data))
            .ToHashSet();
    }
    public float GetOpposingPowerPointsWeighted(Data data)
    {
        return Faces.Select(f => f.GetNative(data))
            .Distinct()
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(Alliance, data))
            .Sum(c => data.Context.PowerPoints[c]);
    }
}