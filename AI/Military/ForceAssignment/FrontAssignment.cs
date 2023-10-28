using System.Collections.Generic;

public class FrontAssignment : ForceAssignment
{
    public Front Front { get; private set; }

    public FrontAssignment(Front front)
    {
        Front = front;
    }

    public override void CalculateOrders(MinorTurnOrders orders, Data data)
    {
        
    }
}