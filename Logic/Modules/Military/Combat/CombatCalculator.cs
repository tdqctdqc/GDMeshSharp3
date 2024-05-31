using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CombatCalculator
{
    public CombatGraph Graph { get; private set; }
    public void Calculate(LogicWriteKey key)
    {
        Graph = new CombatGraph(this);
        key.Data.HostLogicData.CombatGraphIds.Reset();
        SetupGraph(key);
        Graph.DistributeResources(key.Data);
        Graph.CalculateCombat(key.Data);
        Graph.EnactDirectResults(key);
        Graph.EnactInvoluntaryResults(key);
        Graph.EnactVoluntaryResults(key);
    }

    private void SetupGraph(LogicWriteKey key)
    {
        foreach (var group in key.Data.GetAll<Army>())
        {
            group.LineMission.RegisterCombatActions(group, this, key);
            foreach (var other in group.OtherOrders)
            {
                other.RegisterCombatActions(group, this, key);
            }
        }
    }
}