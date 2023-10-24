using System.Collections.Generic;

public class DeploymentAi
{
    public HashSet<ForceAssignment> ForceAssignments { get; private set; }

    public DeploymentAi()
    {
        ForceAssignments = new HashSet<ForceAssignment>();
    }
    public void Calculate(Data data, MinorTurnOrders orders)
    {
        
    }
}