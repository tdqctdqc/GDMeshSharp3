using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FlowTooltipTemplate : TooltipTemplate<(Flow, Regime)>
{
    protected override List<Func<(Flow, Regime), Data, Control>> _fastGetters { get; }
        = new ()
        {
            (fr, d) => NodeExt.CreateLabel("Net: " + fr.Item2.Store.Get(fr.Item1)),
        };

    protected override List<Func<(Flow, Regime), Data, Control>> _slowGetters { get; }
        = new() { };
}
