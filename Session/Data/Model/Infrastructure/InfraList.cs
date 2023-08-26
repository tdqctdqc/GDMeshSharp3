
public class InfraList : ModelList<InfraModel>
{
    public Port Port { get; private set; }

    public InfraList(PeepJobList jobs, Items items)
    {
        Port = new Port();
    }
}