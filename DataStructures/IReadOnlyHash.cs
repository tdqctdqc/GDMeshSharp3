
using System.Collections.Generic;

public interface IReadOnlyHash<T> : IReadOnlyCollection<T>
{
    bool Contains(T t);
}
