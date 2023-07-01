
    using System;
    using Godot;

    public class PlayerAux : EntityAux<Player>
    {
        public Entity1To1Indexer<Player, Regime> ByRegime { get; private set; }
        public Entity1to1PropIndexer<Player, Guid> ByGuid { get; private set; }
        public RefAction<ValChangeNotice<EntityRef<Regime>>> PlayerChangedRegime { get; private set; }
        private Data _data;
        public PlayerAux(Domain domain, Data data) : base(domain, data)
        {
            _data = data;
            var regimeVar = Game.I.Serializer.GetEntityMeta<Player>()
                .GetEntityVarMeta<EntityRef<Regime>>(nameof(Player.Regime));
            PlayerChangedRegime = regimeVar.ValChanged();
            ByRegime = Entity1To1Indexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime, PlayerChangedRegime);
            ByGuid = Entity1to1PropIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[_data.ClientPlayerData.LocalPlayerGuid];
    }
