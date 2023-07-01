
public class ResourceDepositAux : EntityAux<ResourceDeposit>
{
    public EntityMultiIndexer<MapPolygon, ResourceDeposit> ByPoly { get; private set; }
    public ResourceDepositAux(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = new EntityMultiIndexer<MapPolygon, ResourceDeposit>(data, 
            r => r.Poly,
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.MadeResources},
            new RefAction<ValChangeNotice<EntityRef<MapPolygon>>>[]{}
        );
    }
}
