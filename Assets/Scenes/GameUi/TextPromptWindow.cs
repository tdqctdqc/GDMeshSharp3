using System;
using Godot;

public partial class TextPromptWindow : Window
{
    public static void Open(Client client,
        string prompt,
        Action<string> action)
    {
        var w = new TextPromptWindow(prompt, action);
        w.Size = new Vector2I(300, 200);
        client.WindowHolder.OpenWindow(w);
    }
    private TextPromptWindow(string prompt,
        Action<string> action)
    {
        this.MakeFreeable();
        var vbox = new VBoxContainer();
        var entry = new LineEdit();
        vbox.CreateLabelAsChild(prompt);
        vbox.AddChild(entry);
        vbox.AddButton("Enter", () =>
        {
            var s = entry.Text;
            action(s);
            this.QueueFree();
        });
        AddChild(vbox);
        vbox.AnchorsPreset = (int)Control.LayoutPreset.Center;
    }
}