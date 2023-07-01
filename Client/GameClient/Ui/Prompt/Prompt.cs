
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

    public static Prompt CreatePrompt<T>(string descr, List<T> elements, Action<T> action, Func<T, string> descrs)
    {
        return new Prompt(descr, elements.Select<T, Action>(e => () => action(e)).ToList(),
            elements.Select(e => descrs(e)).ToList());
    }

    public static Prompt GetChooseRegimePrompt(WriteKey key)
    {
        var availRegimes = key.Data.Society.Regimes.Entities
            .Where(r => r.IsPlayerRegime(key.Data) == false);
        Action<Regime> action = r =>
        {
            var com = new ChooseRegimeCommand(r.MakeRef());
            key.Session.Server.QueueCommandLocal(com);
        };
        return CreatePrompt<Regime>("Choose Regime", availRegimes.ToList(), action, r => r.Name);
    }
    public Prompt(string descr, List<Action> actions, List<string> actionDescrs)
    {
        Descr = descr;
        Actions = actions;
        ActionDescrs = actionDescrs;
    }
}

