
public abstract class HostProcedure : Message
{
    public abstract void Enact(LogicWriteKey key);
    public abstract bool Valid(Data d, out string error);
}