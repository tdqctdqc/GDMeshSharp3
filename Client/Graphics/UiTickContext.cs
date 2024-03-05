
public class UiTickContext
{
    public float ZoomLevel { get; private set; }

    public UiTickContext(Client client)
    {
        ZoomLevel = client.Cam().ScaledZoomOut;
    }
}