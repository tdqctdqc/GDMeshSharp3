
    using Godot;

    public class SettlementAux : EntityAux<Settlement>
    {
        public EntityPropEntityIndexer<Settlement, MapPolygon> ByPoly { get; private set; }
        public ValChangeAction<ModelRef<SettlementTier>> ChangedTier { get; private set; }

        public SettlementAux(Data data) : base(data)
        {
            ByPoly = EntityPropEntityIndexer<Settlement, MapPolygon>
                .CreateStatic(data, s => s.Poly);
            ChangedTier = new ValChangeAction<ModelRef<SettlementTier>>();
            Game.I.Serializer.GetEntityMeta<Settlement>()
                .GetEntityVarMeta<ModelRef<SettlementTier>>(nameof(Settlement.Tier)).ValChanged()
                .Subscribe(ChangedTier);
        }
    }
