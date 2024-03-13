
    using Godot;

    public class SettlementAux
    {
        public Indexer<Cell, Settlement> ByCell { get; private set; }
        public SettlementAux(Data data)
        {
            ByCell = Indexer.MakeForEntity<Cell, Settlement>
                (s => s.Cell.Get(data), data);
        }
    }
