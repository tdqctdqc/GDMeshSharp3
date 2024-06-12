
public class DoClientCallbackProcedure : Procedure
{
    public int CallbackId { get; private set; }

    public DoClientCallbackProcedure(int callbackId)
    {
        CallbackId = callbackId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Game.I.Client.Callbacks.CallBack(CallbackId, key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}