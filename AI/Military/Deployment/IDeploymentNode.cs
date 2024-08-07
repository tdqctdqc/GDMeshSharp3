
using System;
using System.Collections.Generic;
using Godot;

public interface IDeploymentNode 
{
    ERef<Alliance> Alliance { get; }
    float GetPowerPointsAssigned(Data data);
    float GetPowerPointNeed(Data data);
    void SetWeights(LogicKey key);
    void GiveOrders(LogicKey key);
    Cell GetCharacteristicCell(Data d);
    Army PullGroup(Func<Army, float> suitability,
        LogicKey key);

    void PushGroup(Army g, LogicKey key);
    void Draw(MeshBuilder mb, Vector2 relTo, Data d);
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

