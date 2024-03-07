using System;
using System.Collections.Generic;
using System.Linq;

public class ChooseRegimePrompt : Prompt
{
    public ChooseRegimePrompt(Client client) 
        : base("Choose Regime", new List<Action>(), new List<string>())
    {
        var availRegimes = client.Data.GetAll<Regime>()
            .Where(r => r.IsPlayerRegime(client.Data) == false
                && r.IsMajor);
        Action<Regime> action = r =>
        {
            var com = new ChooseRegimeCommand(r.MakeRef(), 
                client.Data.ClientPlayerData.LocalPlayerGuid);
            client.HandleCommand(com);
        };
        foreach (var r in availRegimes)
        {
            AddAction(() => action(r), r.Name);
        }
    }
}
