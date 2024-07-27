
public class ChangeTemplateTroopAmountProcedure : Procedure
{
    public ERef<UnitTemplate> Template { get; private set; }
    public ModelRef<TroopType> TroopType { get; private set; }
    public float NewAmount { get; private set; }

    public ChangeTemplateTroopAmountProcedure(
        ERef<UnitTemplate> template, ModelRef<TroopType> troopType, float newAmount)
    {
        Template = template;
        TroopType = troopType;
        NewAmount = newAmount;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var template = Template.Get(key.Data);
        template.Troops.Set(TroopType.RefId, NewAmount);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}