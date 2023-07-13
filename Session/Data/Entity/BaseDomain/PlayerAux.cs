
    using System;
    using Godot;

    public class PlayerAux : EntityAux<Player>
    {
        public EntityPropEntityIndexer<Player, Regime> ByRegime { get; private set; }
        public PropEntityIndexer<Player, Guid> ByGuid { get; private set; }
        public ValChangeAction<EntityRef<Regime>> PlayerChangedRegime { get; private set; }
        private Data _data;
        public PlayerAux(Domain domain, Data data) : base(domain, data)
        {
            _data = data;
            var regimeVar = Game.I.Serializer.GetEntityMeta<Player>()
                .GetEntityVarMeta<EntityRef<Regime>>(nameof(Player.Regime));
            PlayerChangedRegime = regimeVar.ValChanged();
            ByRegime = EntityPropEntityIndexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime, PlayerChangedRegime);
            ByGuid = PropEntityIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[_data.ClientPlayerData.LocalPlayerGuid];
    }
