// using System;
// using System.Collections.Generic;
// using System.Linq;
//
// public class DecisionPrompt : Prompt
// {
//     public string Descr { get; set; }
//     public List<Action> Actions { get; set; }
//     public List<string> ActionDescrs { get; set; }
//
//     public DecisionPrompt(Decision d, ClientWriteKey key)
//     {
//         Descr = d.GetDescription();
//         var options = d.GetOptions();
//         Actions = options
//             .Select(o =>
//             {
//                 Action a = () =>
//                 {
//                     var com = new ChooseDecisionCommand(d, o.Name);
//                     key.Session.Server.QueueCommandLocal(com);
//                 };
//                 return a;
//             }).ToList();
//         ActionDescrs = options.Select(o => o.Description).ToList();
//     }
// }
