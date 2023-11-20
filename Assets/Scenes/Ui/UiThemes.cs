using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UiThemes
{
    public static Theme DefaultTheme = GD.Load<Theme>("Assets/Themes/DefaultTheme.tres");
    public static LabelSettings MapLabelSettings = GD.Load<LabelSettings>("Assets/Fonts/MapLabelSettings.tres");
}
