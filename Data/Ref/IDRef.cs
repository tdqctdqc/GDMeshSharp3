
public interface IDRef<T> 
    where T : IIdentifiable
{
    int RefId { get; }
    T Get(Data d);
}