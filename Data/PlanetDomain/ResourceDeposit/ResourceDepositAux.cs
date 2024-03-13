
public class ResourceDepositAux
{
    public Indexer<Cell, ResourceDeposit> ByCell { get; private set; }
    public ResourceDepositAux(Data data)
    {
        ByCell = Indexer.MakeForEntity<Cell, ResourceDeposit>(
            r => r.Cell.Get(data), data);
    }
}
