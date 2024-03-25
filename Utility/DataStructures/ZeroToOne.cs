using System;

public struct ZeroToOne
{
    public float Value { get; private set; }
    public ZeroToOne(float value)
    {
        Value = value;
        if (Value < 0f || Value > 1f) throw new Exception();
    }
}