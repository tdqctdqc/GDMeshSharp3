
using Godot;

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

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(TargetAllianceId) == false)
        {
            error = "Could not find target alliance";
            return false;
        }

        if (data.HasEntity(DeclaringAllianceId) == false)
        {
            error = "Could not find declarer alliance";
            return false;
        }

        if (TargetAllianceId == DeclaringAllianceId)
        {
            error = "Target and declaring alliance are the same";
            return false;
        }
        
        var target = data.Get<Alliance>(TargetAllianceId);
        var declarer = data.Get<Alliance>(DeclaringAllianceId);
        if (target.IsRivals(declarer, data) == false)
        {
            error = "Target and declarer are not rivals";
            return false;
        }
        if (target.IsAtWar(declarer, data))
        {
            error = "Target and declarer are already at war";
            return false;
        }

        error = "";
        return true;
    }
}