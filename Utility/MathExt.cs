using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MathExt
{
    public static float RoundTo2Digits(this float f)
    {
        return f - (f % .01f);
        
        var i = Mathf.FloorToInt(f);

        var tenths = Mathf.FloorToInt((f * 10f) % 10);
        var hundredths = Mathf.FloorToInt((f * 100f) % 10);
        var r = i + tenths * .1f + hundredths * .01f;

        if (f % 1f != 0f)
        {

            throw new Exception();
        }
        
        return i + tenths * .1f + hundredths * .01f;
        
    }
    public static int GetNumDigits(this int i)
    {
        if (i == 0) return 0;
        return (int)Math.Floor((float)Math.Log10(i)) + 1;
    }
    public static float ProjectToRange(this float val, float range, float resultFloor, float cutoff)
    {
        return (val - cutoff) * (range - resultFloor) + resultFloor;
    }
}
