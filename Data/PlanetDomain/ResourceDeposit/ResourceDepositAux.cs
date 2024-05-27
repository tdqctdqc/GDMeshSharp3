
public class ResourceDepositAux
{
    public OneToOneIndexer<Cell, ResourceDeposit> ByCell { get; private set; }
    public ResourceDepositAux(Data data)
    {
        ByCell = OneToOneIndexer.MakeForEntity<Cell, ResourceDeposit>(
            r => r.Cell.Get(data), data);
    }
}
