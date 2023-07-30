using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PromptSideIcon : Button
{
    public void Setup(Prompt prompt)
    {
        CustomMinimumSize = Vector2.One * 100f;
        Text = prompt.Descr;
        Pressed += () => PressedAction(prompt);
        prompt.Satisfied += QueueFree;
    }

    private void PressedAction(Prompt prompt)
    {
        Game.I.Client.GetComponent<PromptManager>().OpenPromptWindow(prompt);
    }
}
