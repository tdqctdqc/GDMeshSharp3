
using System;
using System.Collections.Generic;
using Godot;

public class DeploymentBranchTooltipTemplate : TooltipTemplate<DeploymentBranch>
{
    protected override List<Func<DeploymentBranch, Data, Control>> _fastGetters { get; }
        = new ()
        {
            Get
        };

    protected override List<Func<DeploymentBranch, Data, Control>> _slowGetters { get; }
        = new()
        {

        };

    private static Control Get(DeploymentBranch branch, Data d)
    {

        var panel = new VBoxContainer();
        panel.CreateLabelAsChild(branch.GetType().Name);
        foreach (var c in branch.Assignments)
        {
            panel.CreateLabelAsChild($"\t{c.GetType().Name}");
            if (c is GroupAssignment g)
            {
                panel.CreateLabelAsChild($"\t\tGroups: {g.Groups.Count}");
            }
            panel.CreateLabelAsChild($"\aAssigned {c.GetPowerPointsAssigned(d)}");
            panel.CreateLabelAsChild($"\tNeeded {c.GetPowerPointNeed(d)}");
        }

        return panel;
    }
}