
    using System;
    using Godot;

    public class PlayerAux
    {
        public OneToOneIndexer<Regime, Player> ByRegime { get; private set; }
        public OneToOneIndexer<Guid, Player> ByGuid { get; private set; }
        private Data _data;
        public PlayerAux(Data data)
        {
            _data = data;
            ByRegime = OneToOneIndexer.MakeForEntity<Regime, Player>(
                p => p.Regime.Get(data), data);
            ByRegime.RegisterChanged(data.Notices.Player.PlayerChangedRegime);
            ByGuid = OneToOneIndexer.MakeForEntity<Guid, Player>(
                p => p.PlayerGuid, data);
        }
        public Player LocalPlayer => ByGuid[_data.ClientPlayerData.LocalPlayerGuid];
    }
