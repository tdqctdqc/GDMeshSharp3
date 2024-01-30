
using System.Collections.Generic;
using Godot;

public interface IDeploymentNode : IIdentifiable
{
    DeploymentBranch Parent(Data d);
    IEnumerable<IDeploymentNode> Children();
    float GetPowerPointsAssigned(Data data);
    float GetPowerPointNeed(Data data);
    void GiveOrders(LogicWriteKey key);
    void AdjustWithin(LogicWriteKey key);
    void Disband(LogicWriteKey key);
    UnitGroup GetPossibleTransferGroup(LogicWriteKey key);
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

