
public class DestroyArmyProcedure : Procedure
{
    public DestroyArmyProcedure(ERef<Army> army)
    {
        Army = army;
    }

    public ERef<Army> Army { get; private set; }
    public override void Enact(ProcedureWriteKey key)
    {
        var army = Army.Get(key.Data);
        foreach (var item in army.Units.Items(key.Data))
        {
            key.Data.RemoveEntity(item.Id, key);
        }
        key.Data.RemoveEntity(Army.RefId, key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}