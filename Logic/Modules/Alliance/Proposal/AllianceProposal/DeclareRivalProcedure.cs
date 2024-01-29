
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
        a.Rivals.Add(t, key);
        t.Rivals.Add(a, key);
    }

    public override bool Valid(Data data)
    {
        return AllianceId != TargetAllianceId 
               && data.HasEntity(TargetAllianceId)
               && data.HasEntity(AllianceId);
    }
}