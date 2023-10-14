
using System.Linq;

public class FormUnitProcedure : Procedure
{
    public EntityRef<Regime> Regime { get; private set; }
    public EntityRef<UnitTemplate> Template { get; private set; }
    public override void Enact(ProcedureWriteKey key)
    {
        
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}