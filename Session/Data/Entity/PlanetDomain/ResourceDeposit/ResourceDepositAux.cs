
public class ResourceDepositAux : EntityAux<ResourceDeposit>
{
    public EntityMultiIndexer<MapPolygon, ResourceDeposit> ByPoly { get; private set; }
    public ResourceDepositAux(Data data) : base(data)
    {
        ByPoly = new EntityMultiIndexer<MapPolygon, ResourceDeposit>(data, 
            r => r.Poly.Entity(data),
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.MadeResources},
            new ValChangeAction<MapPolygon>[]{}
        );
    }
}
