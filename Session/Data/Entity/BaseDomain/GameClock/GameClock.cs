
using System;
using Godot;
using MessagePack;

public class GameClock : Entity
{
    public int Tick { get; protected set; }
    public static GameClock Create(GenWriteKey key)
    {
        var gc = new GameClock(-1, 0);
        key.Create(gc);
        return gc;
    }
    [SerializationConstructor] private GameClock(int id, int tick) : base(id)
    {
        Tick = tick;
    }

    public bool MajorTurn(Data data)
    {
        return Tick % data.BaseDomain.Rules.TickCycleLength == 0;
    }

    public void DoTick(ProcedureWriteKey key)
    {
        Tick++;
        key.Data.Notices.Ticked.Invoke(Tick);
    }

}
