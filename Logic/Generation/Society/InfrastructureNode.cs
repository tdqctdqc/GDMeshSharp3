public class InfrastructureNode
{
    public Cell Cell { get; private set; }
    public float Size { get; private set; }

    public InfrastructureNode(Cell cell, float size)
    {
        Cell = cell;
        Size = size;
    }
}

public class InfraNodeEdge
{
    public float Cost { get; set; }
    public float Traffic { get; set; }
    public float Length { get; set; }
    public InfraNodeEdge(float cost, float traffic,
        float length)
    {
        Cost = cost;
        Traffic = traffic;
        Length = length;
    }
}