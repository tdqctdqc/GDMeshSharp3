using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FlowTooltipTemplate : TooltipTemplate<(Flow, Regime)>
{
    protected override List<Func<(Flow, Regime), Data, Control>> _fastGetters { get; }
        = new ()
        {
            (fr, d) => NodeExt.CreateLabel("In: " + fr.Item2.Flows.Get(fr.Item1).FlowIn),
            (fr, d) => NodeExt.CreateLabel("Out: " + fr.Item2.Flows.Get(fr.Item1).FlowOut),
            (fr, d) => NodeExt.CreateLabel("Net: " + fr.Item2.Flows.Get(fr.Item1).Net()),
        };

    protected override List<Func<(Flow, Regime), Data, Control>> _slowGetters { get; }
        = new() { };
}
