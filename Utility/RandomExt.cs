using Godot;
using System;

public static class RandomExt 
{
    public static T GetWeighted<T>(this RandomNumberGenerator rand, T t1, float chance1, T t2, float chance2)
    {
        float total = chance1 + chance2;
        var sample = rand.RandfRange(0, total);
        if (sample < chance1) return t1;
        return t2;
    }
}
