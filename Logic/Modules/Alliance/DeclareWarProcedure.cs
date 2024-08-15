
using Godot;

public class DeclareWarProcedure : Procedure
{
    public ERef<Regime> Target { get; private set; }
    public ERef<Regime> Declarer { get; private set; }

    public DeclareWarProcedure(ERef<Regime> target, ERef<Regime> declarer)
    {
        Target = target;
        Declarer = declarer;
    }

    public override void Enact(ProcedureKey key)
    {
        var declarer = Declarer.Get(key.Data);
        var target = Target.Get(key.Data);
        key.Data.Society.DiploGraph.AddEdge(declarer, target, DiploRelation.War, key);            
        key.Data.Notices.Political.WarDeclared.Invoke((declarer, target));
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(Target.RefId) == false)
        {
            error = "Could not find target alliance";
            return false;
        }

        if (data.HasEntity(Declarer.RefId) == false)
        {
            error = "Could not find declarer alliance";
            return false;
        }

        if (Target.RefId == Declarer.RefId)
        {
            error = "Target and declaring alliance are the same";
            return false;
        }
        
        var declarer = Declarer.Get(data);
        var target = Target.Get(data);
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