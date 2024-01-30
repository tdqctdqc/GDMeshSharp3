using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeploymentAi
{
    private Dictionary<int, IDeploymentNode> _nodesById;
    public DeploymentRoot Root { get; private set; }
    public static DeploymentAi Construct(Regime r, Data d)
    {
        return new DeploymentAi(DeploymentRoot.Construct(r, d));
    }
    [SerializationConstructor] private DeploymentAi(DeploymentRoot root)
    {
        Root = root;
        SetupIds();
    }
    public void Calculate(Regime regime, LogicWriteKey key, MinorTurnOrders orders)
    {
        Root.MakeTheaters(key);
        //shift groups thru reserves
        
        
        Root.AdjustWithin(key);
    }
    private void SetupIds()
    {
        _nodesById = new Dictionary<int, IDeploymentNode>();
        var queue = new Queue<IDeploymentNode>();
        queue.Enqueue(Root);
        while (queue.TryDequeue(out var curr))
        {
            _nodesById.Add(curr.Id, curr);
            foreach (var c in curr.Children())
            {
                queue.Enqueue(c);
            }
        }
    }
    

    public IDeploymentNode GetNode(int id)
    {
        return _nodesById[id];
    }

    public void RemoveNode(int id, LogicWriteKey key)
    {
        _nodesById.Remove(id);
    }
}