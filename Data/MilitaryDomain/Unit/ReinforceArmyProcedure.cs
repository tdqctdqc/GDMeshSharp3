
using Godot;

public class ReinforceArmyProcedure : Procedure
{
    public ERef<Army> Army { get; private set; }

    public ReinforceArmyProcedure(ERef<Army> army)
    {
        Army = army;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var army = Army.Get(key.Data);
        foreach (var unit in army.Units.Entities(key.Data))
        {
            var template = unit.Template.Get(key.Data);
            var regime = unit.Regime.Get(key.Data);
            var reserve = regime.Stock;

            foreach (var (troop, count) in unit.Troops.GetEnumModel(key.Data))
            {
                if (reserve.Stock.Contents.ContainsKey(troop.Id) == false)
                {
                    continue;
                }

                var need = template.Troops.Get(troop);
                if (count >= need) continue;

                var transfer = Mathf.Clamp(need - count, 
                    0f, reserve.Stock.Get(troop));
                if (transfer > 0)
                {
                    reserve.Stock.Remove(troop, transfer);
                    unit.Troops.Add(troop, transfer);
                }
            }
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        if (data.HasEntity(Army.RefId) == false)
        {
            error = "army not found";
            return false;
        }
        return true;
    }
}