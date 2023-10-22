
public interface ICoastWaypoint : IWaypoint
{
    int Sea { get; }
    bool Port { get; }
    void SetPort(bool port, GenWriteKey key);
}