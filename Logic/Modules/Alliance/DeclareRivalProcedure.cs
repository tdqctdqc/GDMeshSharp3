
using Godot;

public class DeclareRivalProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public ERef<Regime> TargetRegime { get; private set; }

    public DeclareRivalProcedure(ERef<Regime> regime, ERef<Regime> targetRegime)
    {
        Regime = regime;
        TargetRegime = targetRegime;
    }

    public override void Enact(ProcedureKey key)
    {
        var a = Regime.Get(key.Data);
        var t = TargetRegime.Get(key.Data);
        key.Data.Society.DiploGraph.AddEdge(a, t, DiploRelation.Rivals, key);
    }

    public override bool Valid(Data data, out string error)
    {
        if (Regime.RefId == TargetRegime.RefId)
        {
            error = "Proposer and target are same alliance";
            return false;
        }

        if (data.HasEntity(TargetRegime.RefId) == false)
        {
            error = "Target alliance not found";
            return false;
        }
        if (data.HasEntity(Regime.RefId) == false)
        {
            error = "Proposer alliance not found";
            return false;
        }

        error = "";
        return true;
    }
}