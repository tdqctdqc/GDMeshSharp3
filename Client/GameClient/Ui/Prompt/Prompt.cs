
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Prompt
{
    public string Descr { get; private set; }
    public Action Satisfied { get; set; }
    public List<Action> Actions { get; private set; }
    public List<string> ActionDescrs { get; private set; }
    public Prompt(string descr, List<Action> actions, List<string> actionDescrs)
    {
        Descr = descr;
        Actions = actions;
        ActionDescrs = actionDescrs;
    }
    protected void AddAction(Action action, string descr)
    {
        Actions.Add(action);
        ActionDescrs.Add(descr);
    }
}

