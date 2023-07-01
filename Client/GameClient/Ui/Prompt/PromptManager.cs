
using System;
using System.Collections.Generic;
using Godot;

public class PromptManager
{
    private Dictionary<Prompt, PromptWindow> _windows;
    private float _timer;
    private float _period = 1f;
    private GameUi _gameUi;

    public PromptManager(GameUi gameUi, Data data)
    {
        _gameUi = gameUi;
        _windows = new Dictionary<Prompt, PromptWindow>();
    }

    public void AddPromptIcon(Prompt prompt)
    {
        var icon = new PromptSideIcon();
        icon.Setup(prompt, _gameUi);
        _gameUi.PromptSidebar.AddPromptIcon(icon);
    }
    public void OpenPromptWindow(Prompt prompt)
    {
        if (_windows.ContainsKey(prompt) == false)
        {
            var w = SceneManager.Instance<PromptWindow>();
            w.Setup(prompt);
            _gameUi.AddChild(w);
            w.PopupCentered();
            _windows.Add(prompt, w);
            w.CloseRequested += () => _windows.Remove(prompt);
        }
    }
}
