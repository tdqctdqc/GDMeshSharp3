using Godot;
using System;

public class ColorDispenser
{
    private int _index;

    public Color GetColor()
    {
        _index++;
        return ColorsExt.GetRainbowColor(_index);
    }
}
