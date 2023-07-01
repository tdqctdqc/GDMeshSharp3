using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

public static class ColorsExt
{
    public static List<Color> ColorList
        = GetColors();

    public static List<Color> Rainbow
        = GetRainbow();

    public static Color Transparent = new Color(0f, 0f, 0f, 0f);

    private static RandomNumberGenerator _rand = new RandomNumberGenerator();

    private static List<Color> GetRainbow()
    {
        var list = new List<Color>();
        list.Add(Colors.Red);
        list.Add(Colors.Orange);
        list.Add(Colors.Yellow);
        list.Add(Colors.Green);
        list.Add(Colors.Blue);
        list.Add(Colors.Purple);
        return list;
    }

    public static Color GetRandomScale()
    {
        
        var rand = _rand.RandfRange(0f, 1f);
        return new Color(
            rand,rand,rand
        );
    }

public static Color GetRainbowColor(int index)
    {
        return Rainbow[index % Rainbow.Count];
    }

    public static Color GetRandomColor()
    {
        return new Color(
            _rand.RandfRange(0f, 1f),
            _rand.RandfRange(0f, 1f),
            _rand.RandfRange(0f, 1f)
        );
    }

    public static Color GetPeriodicShade(this Color color, int iter)
    {
        return color.Darkened((iter % 7) / 7f);
    }

public static Color GetColor(int index)
    {
        return ColorList[index % ColorList.Count];
    }
    private static List<Color> GetColors()
    {
        var colors = new List<Color>();
        var assembly = Assembly.GetExecutingAssembly();
        var godotColorsType = typeof(Colors);
        var props = godotColorsType.GetProperties();
        var colorType = typeof(Color);

        foreach (var prop in props)
        {
            if (prop.PropertyType == colorType)
            {
                colors.Add((Color)prop.GetValue(null));
            }
        }

        return colors;
    }
}
