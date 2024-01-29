
    using System;
    using Godot;

    public class PlayerAux
    {
        public PropEntityIndexer<Player, Regime> ByRegime { get; private set; }
        public PropEntityIndexer<Player, Guid> ByGuid { get; private set; }
        public ValChangeAction<Player, Regime> PlayerChangedRegime { get; private set; }
        public RefAction SetLocalPlayer { get; private set; }
        private Data _data;
        public PlayerAux(Data data)
        {
            _data = data;
            PlayerChangedRegime = new ValChangeAction<Player, Regime>();
            ByRegime = PropEntityIndexer<Player, Regime>.CreateDynamic(data, 
                p => p.Regime.Entity(data), PlayerChangedRegime);
            ByGuid = PropEntityIndexer<Player, Guid>.CreateConstant(data, p => p.PlayerGuid);
            SetLocalPlayer = new RefAction();
            data.SubscribeForCreation<Player>(p =>
            {
                if (((Player) p.Entity).PlayerGuid == data.ClientPlayerData.LocalPlayerGuid)
                {
                    SetLocalPlayer.Invoke();
                }
            });
        }

        public Player LocalPlayer => ByGuid[_data.ClientPlayerData.LocalPlayerGuid];
    }
