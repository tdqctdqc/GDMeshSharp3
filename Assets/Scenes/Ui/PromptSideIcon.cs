using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PromptSideIcon : Button
{
    public void Setup(Prompt prompt, GameUi ui)
    {
        CustomMinimumSize = Vector2.One * 100f;
        Text = prompt.Descr;
        Pressed += () => PressedAction(prompt, ui);
        prompt.Satisfied += QueueFree;
    }

    private void PressedAction(Prompt prompt, GameUi ui)
    {
        ui.Prompts.OpenPromptWindow(prompt);
    }
}
