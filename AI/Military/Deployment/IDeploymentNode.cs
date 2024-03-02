
using System;
using System.Collections.Generic;
using Godot;

public interface IDeploymentNode 
{
    Regime Regime { get; }
    float GetPowerPointsAssigned(Data data);
    float GetPowerPointNeed(Data data);
    void GiveOrders(DeploymentAi ai, LogicWriteKey key);
    Cell GetCharacteristicCell(Data d);
    UnitGroup PullGroup(DeploymentAi ai, 
        Func<UnitGroup, float> suitability,
        LogicWriteKey key);

    void PushGroup(DeploymentAi ai, UnitGroup g, LogicWriteKey key);
}

public static class IDeploymentNodeExt
{
    public static float GetSatisfiedRatio(this IDeploymentNode n,
        Data d)
    {
        var assigned = n.GetPowerPointsAssigned(d);
        var need = n.GetPowerPointNeed(d);
        if (need == 0f) return Mathf.Inf;
        return assigned / need;
    }
}

