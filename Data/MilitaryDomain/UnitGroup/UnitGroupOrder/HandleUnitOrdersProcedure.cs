
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class HandleUnitOrdersProcedure : Procedure
{
    public ConcurrentDictionary<int, MapPos> NewUnitPosesById { get; private set; }
    public static HandleUnitOrdersProcedure Construct()
    {
        return new HandleUnitOrdersProcedure(new ConcurrentDictionary<int, MapPos>());
    }

    [SerializationConstructor] private HandleUnitOrdersProcedure(
        ConcurrentDictionary<int, MapPos> newUnitPosesById)
    {
        NewUnitPosesById = newUnitPosesById;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var kvp in NewUnitPosesById)
        {
            var unit = key.Data.Get<Unit>(kvp.Key);
            unit.SetPosition(kvp.Value, key);
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}