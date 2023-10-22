
public interface ILandWaypoint : IWaypoint
{
    float Roughness { get; }
    void SetRoughness(float roughness, GenWriteKey key);
}