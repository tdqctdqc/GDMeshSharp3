
    using Godot;

    public class SettlementAux
    {
        public OneToOneIndexer<Cell, Settlement> ByCell { get; private set; }
        public SettlementAux(Data data)
        {
            ByCell = OneToOneIndexer.MakeForEntity<Cell, Settlement>
                (s => s.Cell.Get(data), data);
        }
    }
