using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GeneratorSettingsWindow : SettingsWindow
{
    public static void Open(
        GenerationMultiSettings settings)
    {
        var w = new GeneratorSettingsWindow();
        w.Setup(settings);
        Game.I.Client.WindowHolder.OpenWindowFullSize(w);
    }

    private GeneratorSettingsWindow()
    {
    }
}
