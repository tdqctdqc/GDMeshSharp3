
    using Godot;

    public class SettlementAux
    {
        public EntityPropEntityIndexer<Settlement, MapPolygon> ByPoly { get; private set; }
        public ValChangeAction<Settlement, SettlementTier> ChangedTier { get; private set; }
        public SettlementAux(Data data)
        {
            ByPoly = EntityPropEntityIndexer<Settlement, MapPolygon>
                .CreateStatic(data, s => s.Poly);
            ChangedTier = new ValChangeAction<Settlement, SettlementTier>();
        }
    }
