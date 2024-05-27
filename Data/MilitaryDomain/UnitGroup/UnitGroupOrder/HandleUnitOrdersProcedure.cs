
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class HandleUnitOrdersProcedure : Procedure
{
    public ConcurrentDictionary<int, HashSet<int>> NewArmyPosesById { get; private set; }
    public static HandleUnitOrdersProcedure Construct()
    {
        return new HandleUnitOrdersProcedure(new ConcurrentDictionary<int, HashSet<int>>());
    }

    [SerializationConstructor] private HandleUnitOrdersProcedure(
        ConcurrentDictionary<int, HashSet<int>> newArmyPosesById)
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