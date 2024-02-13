using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using System.Reflection.Metadata;
using Godot;
using MessagePack;

public class FrontSegment : DeploymentBranch
{
    public static float CoverOpposingWeight {get; private set;} = .5f;
    public static float CoverLengthWeight {get; private set;} = 1f;
    public static float DesiredOpposingPpRatio {get; private set;} = 2f;
    public static float PowerPointsPerCellFaceToCover {get; private set;} = 100f;

    public static int IdealSegmentLength = 5;
    public Frontline Frontline { get; private set; }
    public HoldLineAssignment HoldLine { get; private set; }
    public Color Color { get; private set; }
    public bool Attack { get; private set; }
    public FrontSegment(
        DeploymentAi ai,
        List<FrontFace> frontLine,
        bool attack,
        LogicWriteKey key) : base(ai, key)
    {
        var frontLineFaces = frontLine.ToList();
        Color = ColorsExt.GetRandomColor();
        Attack = attack;
        Frontline = new Frontline(frontLineFaces);
        HoldLine = new HoldLineAssignment(ai, this, key);
    }

    public override Vector2 GetMapPosForDisplay(Data d)
    {
        return d.Planet.GetAveragePosition(Frontline.Faces.Select(f => f.GetNative(d).GetCenter()));
    }
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return Frontline.Faces
            .Select(face => PlanetDomainExt.GetPolyCell(face.Native, d)).Distinct();
    }
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = GetLength(data);

        var oppNeed = opposing * DesiredOpposingPpRatio;
        var lengthNeed = length * PowerPointsPerCellFaceToCover;

        return Mathf.Max(oppNeed, lengthNeed);
    }
    public float GetOpposingPowerPoints(Data data)
    {
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetCells(data)
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(alliance, data))
            .Sum(n =>
            {
                var us = n.GetUnits(data);
                if (us == null) return 0f;
                return us.Sum(u => u.GetPowerPoints(data));
            });
    }

    public int GetLength(Data d)
    {
        return Frontline.Faces.Count;
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d).First();
    }
}