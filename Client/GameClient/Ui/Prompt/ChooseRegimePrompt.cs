using System;
using System.Collections.Generic;
using System.Linq;

public class ChooseRegimePrompt : Prompt
{
    public ChooseRegimePrompt(Data data) 
        : base("Choose Regime", new List<Action>(), new List<string>())
    {
        var availRegimes = data.GetAll<Regime>()
            .Where(r => r.IsPlayerRegime(data) == false);
        Action<Regime> action = r =>
        {
            var com = new ChooseRegimeCommand(r.MakeRef(), 
                data.ClientPlayerData.LocalPlayerGuid);
            Game.I.Client.Key.Session.Server.QueueCommandLocal(com);
        };
        foreach (var r in availRegimes)
        {
            AddAction(() => action(r), r.Name);
        }
    }
}
