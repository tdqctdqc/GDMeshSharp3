
using System.Collections.Generic;

public interface IPickerAgent<T>
{
    bool Pick(Picker<T> host, Data data);
    HashSet<T> Picked { get; }
}