
    using System;
    using Godot;

    public class PlayerAux
    {
        public PropEntityIndexer<Player, Regime> ByRegime { get; private set; }
        public PropEntityIndexer<Player, Guid> ByGuid { get; private set; }
        public ValChangeAction<Regime> PlayerChangedRegime { get; private set; }
        private Data _data;
        public PlayerAux(Data data)
        {
            _data = data;
            PlayerChangedRegime = new ValChangeAction<Regime>();
            ByRegime = PropEntityIndexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime.Entity(data), PlayerChangedRegime);
            ByGuid = PropEntityIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
        }

        public Player LocalPlayer => ByGuid[_data.ClientPlayerData.LocalPlayerGuid];
    }
