using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ShapingFunctions
{
    // https://easings.net/
    public static float EaseInOutCubic(float x, float height, float min)
    {
        var raw = x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        return (height - min) * raw + min;
    }
    public static float EaseOutCubic(float x, float height, float floor)
    {
        var raw = 1f - Mathf.Pow(1f - x, 3f);
        return (height - floor) * raw + floor;
    }
    public static float EaseInCubic(float x, float height, float floor)
    {
        var raw = x * x * x;
        return (height - floor) * raw + floor;
    }
    public static float EaseInCubicGuard(float x, float xMin, float xMax, float scaleHeight, float scaleFloor)
    {
        x = Mathf.Clamp(x, xMin, xMax);
        var raw = x * x * x;
        return (scaleHeight - scaleFloor) * raw + scaleFloor;
    }

    public static float ProjectToRange(float input, float inputMax, float outputMin, float outputMax)
    {
        var ratio = input / inputMax;
        ratio = Mathf.Min(1f, ratio);
        var range = outputMax - outputMin;
        return outputMin + range * ratio;
    }
}
