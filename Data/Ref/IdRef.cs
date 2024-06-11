
public interface IdRef 
{
    int RefId { get; }
    IIdentifiable Get(Data d);
}