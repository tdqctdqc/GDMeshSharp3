
using System;
using System.Collections.Generic;
using Godot;

public interface IDeploymentNode : IIdentifiable
{
    ERef<Regime> Regime { get; }
    DeploymentBranch Parent(DeploymentAi ai, Data d);
    IEnumerable<IDeploymentNode> Children();
    float GetPowerPointsAssigned(Data data);
    float GetPowerPointNeed(Data data);
    void GiveOrders(DeploymentAi ai, LogicWriteKey key);
    void AdjustWithin(DeploymentAi ai, LogicWriteKey key);
    void Disband(DeploymentAi ai, LogicWriteKey key);
    bool PullGroup(DeploymentAi ai, GroupAssignment transferTo, LogicWriteKey key);
    void DissolveInto(DeploymentAi ai, DeploymentBranch into,
        LogicWriteKey key);
    PolyCell GetCharacteristicCell(Data d);
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

