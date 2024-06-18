
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class HandleUnitMissionsProcedure : Procedure
{
    public ConcurrentDictionary<int, RefSet<CellRef>> NewArmyPosesById { get; private set; }
    public static HandleUnitMissionsProcedure Construct()
    {
        return new HandleUnitMissionsProcedure(new ConcurrentDictionary<int, RefSet<CellRef>>());
    }

    [SerializationConstructor] private HandleUnitMissionsProcedure(
        ConcurrentDictionary<int, RefSet<CellRef>> newArmyPosesById)
    {
        NewArmyPosesById = newArmyPosesById;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var kvp in NewArmyPosesById)
        {
            var army = key.Data.Get<Army>(kvp.Key);
            army.SetCells(kvp.Value, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}