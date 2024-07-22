
public class ChangeTemplateTroopAmountProcedure : Procedure
{
    public ERef<UnitTemplate> Template { get; private set; }
    public ModelRef<Troop> Troop { get; private set; }
    public float NewAmount { get; private set; }

    public ChangeTemplateTroopAmountProcedure(ERef<UnitTemplate> template, ModelRef<Troop> troop, float newAmount)
    {
        Template = template;
        Troop = troop;
        NewAmount = newAmount;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var template = Template.Get(key.Data);
        template.Troops.Set(Troop.RefId, NewAmount);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}