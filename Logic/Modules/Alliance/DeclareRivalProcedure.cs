
using Godot;

public class DeclareRivalProcedure : Procedure
{
    public int AllianceId { get; private set; }
    public int TargetAllianceId { get; private set; }

    public DeclareRivalProcedure(int allianceId, int targetAllianceId)
    {
        AllianceId = allianceId;
        TargetAllianceId = targetAllianceId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var a = key.Data.Get<Alliance>(AllianceId);
        var t = key.Data.Get<Alliance>(TargetAllianceId);
        key.Data.Society.DiploGraph.AddEdge(a, t, DiploRelation.Rivals, key);
        key.Data.Notices.Political.RivalryDeclared.Invoke((a,t));
    }

    public override bool Valid(Data data, out string error)
    {
        if (AllianceId == TargetAllianceId)
        {
            error = "Proposer and target are same alliance";
            return false;
        }

        if (data.HasEntity(TargetAllianceId) == false)
        {
            error = "Target alliance not found";
            return false;
        }
        if (data.HasEntity(AllianceId) == false)
        {
            error = "Proposer alliance not found";
            return false;
        }

        error = "";
        return true;
    }
}