
using System;
using System.Collections.Generic;
using Godot;

public class GroupAssignmentTooltipTemplate : TooltipTemplate<GroupAssignment>
{
    
    protected override List<Func<GroupAssignment, Data, Control>> _fastGetters { get; }
        = new ()
        {
            Get
        };

    protected override List<Func<GroupAssignment, Data, Control>> _slowGetters { get; }
        = new()
        {

        };

    private static Control Get(GroupAssignment ga, Data d)
    {
        var panel = new VBoxContainer();
        panel.CreateLabelAsChild(ga.GetType().Name);
        panel.CreateLabelAsChild($"\t\tGroups: {ga.Groups.Count}");
        panel.CreateLabelAsChild($"\aAssigned {ga.GetPowerPointsAssigned(d)}");
        panel.CreateLabelAsChild($"\tNeeded {ga.GetPowerPointNeed(d)}");
        
        return panel;
    }
}