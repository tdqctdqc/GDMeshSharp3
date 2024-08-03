
using System.Collections.Generic;
using System.Linq;

public class FrontlineAi
{
    public ERef<Frontline> Frontline { get; private set; }
    public float AttackWeight { get; private set; }
    public float DefendWeight { get; private set; }
    public Dictionary<FrontFace, float> FaceAttackWeights { get; private set; }
    public Dictionary<FrontFace, float> FaceDefendWeights { get; private set; }
    public FrontlineTacticalReport Report { get; private set; }

    public static FrontlineAi Construct(Frontline fl)
    {
        return new FrontlineAi(
            fl.MakeRef(),
            0f, 0f, new Dictionary<FrontFace, float>(),
            new Dictionary<FrontFace, float>(),
            null);
    }
    public FrontlineAi(ERef<Frontline> frontline, float attackWeight, float defendWeight, Dictionary<FrontFace, float> faceAttackWeights, Dictionary<FrontFace, float> faceDefendWeights, FrontlineTacticalReport report)
    {
        Frontline = frontline;
        AttackWeight = attackWeight;
        DefendWeight = defendWeight;
        FaceAttackWeights = faceAttackWeights;
        FaceDefendWeights = faceDefendWeights;
        Report = report;
    }
    
    public void AddAttackWeight(float w,
        Cell attack, LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);

        AttackWeight += w;
        for (var i = 0; i < frontline.Faces.Count; i++)
        {
            var face = frontline.Faces[i];
            if (face.Foreign == attack.Id)
            {
                FaceAttackWeights.AddOrSum(face, w);
            }
        }

        var newAdvanceInto = frontline.
            AdvanceInto.Concat(attack.MakeRef().Yield())
                .ToHashSet();
        var proc = new SetFrontlineAdvanceIntoProcedure(Frontline, newAdvanceInto);
        key.SendMessage(proc);
    }
    public void AddAttackWeight(float w,
        IEnumerable<Cell> attack, LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        AttackWeight += w;
        var ids = attack.Select(a => a.Id).ToHashSet();
        
        for (var i = 0; i < frontline.Faces.Count; i++)
        {
            var face = frontline.Faces[i];
            if (ids.Contains(face.Foreign))
            {
                FaceAttackWeights.AddOrSum(face, w);
            }
        }
        var newAdvanceInto = frontline.
            AdvanceInto.Union(attack.Select(a => a.MakeRef()))
                .ToHashSet();
        var proc = new SetFrontlineAdvanceIntoProcedure(Frontline, newAdvanceInto);
        key.SendMessage(proc);
    }
    
    public void AddDefendWeightAlongWholeLine(float w, Data d)
    {
        var frontline = Frontline.Get(d);
        var alliance = frontline.Alliance.Get(d);
        DefendWeight += w;
        var totalCellDef = frontline.Faces.Sum(
            f => 1f / ((LandCell)f.GetNative(d)).GetLandDefendScore(d));
        
        for (var i = 0; i < frontline.Faces.Count; i++)
        {
            var face = frontline.Faces[i];
            var native = (LandCell)face.GetNative(d);
            var foreign = face.GetForeign(d);
            var mult = foreign.Controller.Get(d)
                .GetAlliance(d).IsAtWar(alliance, d)
                ? 1f : global::Frontline.DefMultForNotAtWarCell;
            if (totalCellDef == 0f)
            {
                FaceDefendWeights.AddOrSum(face, w / frontline.Faces.Count);
            }
            else
            {
                var cellDefRatio = (1f / native.GetLandDefendScore(d)) / totalCellDef;
                FaceDefendWeights.AddOrSum(face, cellDefRatio * w);
            }
        }
    }

    public void MakeReport(Data d)
    {
        Report = new FrontlineTacticalReport(Frontline.Get(d), d);
    }
}