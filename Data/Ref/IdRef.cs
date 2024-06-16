
public interface IdRef 
{
    int RefId { get; }
    IIdentifiable Get(Data d);
}

public static class IdRefExt
{
    public static bool IsEmpty(this IdRef i)
    {
        return i.RefId == -1;
    }

    public static bool Fulfilled(this IdRef i)
    {
        return i.RefId != -1;
    }
}