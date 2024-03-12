
public class ResourceDepositAux
{
    public EntityMultiIndexer<MapPolygon, ResourceDeposit> ByPoly { get; private set; }
    public ResourceDepositAux(Data data)
    {
        ByPoly = new EntityMultiIndexer<MapPolygon, ResourceDeposit>(data, 
            r => r.Poly.Get(data),
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.MadeResources}
        );
    }
}
