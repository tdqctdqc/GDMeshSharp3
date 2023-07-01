
using System;

public class UIVar<T>
{
    public Action<T> ChangedValue { get; set; }
    public T Value { get; private set; }

    public UIVar(T value)
    {
        Value = value;
    }

    public void SetValue(T val)
    {
        Value = val;
        ChangedValue?.Invoke(val);
    }

    public void Clear()
    {
        Value = default;
        ChangedValue = null;
    }
}
