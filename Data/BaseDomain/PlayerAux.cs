
    using System;
    using Godot;

    public class PlayerAux
    {
        public Indexer<Regime, Player> ByRegime { get; private set; }
        public Indexer<Guid, Player> ByGuid { get; private set; }
        private Data _data;
        public PlayerAux(Data data)
        {
            _data = data;
            ByRegime = Indexer.MakeForEntity<Regime, Player>(
                p => p.Regime.Get(data), data);
            ByRegime.RegisterChanged(data.Notices.Player.PlayerChangedRegime);
            ByGuid = Indexer.MakeForEntity<Guid, Player>(
                p => p.PlayerGuid, data);
        }
        public Player LocalPlayer => ByGuid[_data.ClientPlayerData.LocalPlayerGuid];
    }
