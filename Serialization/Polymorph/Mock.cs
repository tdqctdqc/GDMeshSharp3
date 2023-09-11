using System;
using Godot;

public class Mock<T>
{
    public int Extra { get; private set; }
    public T Value { get; private set; }
    // public T Value() => _t;
    public static Mock<T> Construct(T t, int extra)
    {
        return new Mock<T>(t, extra);
    }
    public Mock(T value, int extra)
    {
        Extra = extra;
        Value = value;
    }
}