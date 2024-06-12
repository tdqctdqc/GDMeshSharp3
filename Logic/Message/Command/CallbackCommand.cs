
using System;
using Godot;
using MessagePack;

public class CallbackCommand : Command
{
    public Command Inner { get; private set; }
    public int CallbackId { get; private set; }

    public static CallbackCommand Construct(Command inner, 
        Action callback,
        Client c)
    {
        var player = c.Data.BaseDomain.PlayerAux
            .LocalPlayer.PlayerGuid;
        var id = c.Callbacks.AddCallback(callback);
        return new CallbackCommand(player, inner, id);
    }
    [SerializationConstructor] private CallbackCommand(
        Guid commandingPlayerGuid,
        Command inner,
        int callbackId) : base(commandingPlayerGuid)
    {
        Inner = inner;
        CallbackId = callbackId;
    }

    public override bool Valid(Data data, out string error)
    {
        var valid = Inner.Valid(data, out var e);
        error = e;
        return valid;
    }

    public override void Enact(LogicWriteKey key)
    {
        Inner.Enact(key);
        key.SendMessageToClient(new DoClientCallbackProcedure(CallbackId), CommandingPlayerGuid);
    }
}