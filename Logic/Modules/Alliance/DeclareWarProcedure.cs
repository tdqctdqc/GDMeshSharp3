
public class DeclareWarProcedure : Procedure
{
    public int TargetAllianceId { get; private set; }
    public int DeclaringAllianceId { get; private set; }
    public static DeclareWarProcedure 
        Construct(Alliance declaringAlliance, Alliance targetAlliance,
            Data data)
    {
        return new DeclareWarProcedure(targetAlliance.Id, declaringAlliance.Id);
    }
    public DeclareWarProcedure(int targetAllianceId, int declaringAllianceId)
    {
        TargetAllianceId = targetAllianceId;
        DeclaringAllianceId = declaringAllianceId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var alliance = key.Data.Get<Alliance>(DeclaringAllianceId);
        var target = key.Data.Get<Alliance>(TargetAllianceId);
        key.Data.Society.DiploGraph.AddEdge(alliance, target, DiploRelation.War, key);            
    }

    public override bool Valid(Data data)
    {
        if (data.HasEntity(TargetAllianceId) == false) return false;
        if (data.HasEntity(DeclaringAllianceId) == false) return false;
        if (TargetAllianceId == DeclaringAllianceId) return false;
        
        var target = data.Get<Alliance>(TargetAllianceId);
        var declarer = data.Get<Alliance>(DeclaringAllianceId);
        if (target.IsRivals(declarer, data) == false) return false;
        if (target.IsAtWar(declarer, data)) return false;
        return true;
    }
}