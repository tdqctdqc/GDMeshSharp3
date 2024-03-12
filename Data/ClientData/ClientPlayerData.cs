using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClientPlayerData
{
    public Guid LocalPlayerGuid { get; private set; }
    public MajorTurnOrders MajorOrders { get; private set; }
    public MinorTurnOrders MinorOrders { get; private set; }
    public List<Issue> Issues { get; private set; }
    public Dictionary<Cell, List<Vector2[]>> RiverCells { get; private set; }
    public ClientPlayerData(Data data)
    {
        Issues = new List<Issue>();
        RiverCells = new Dictionary<Cell, List<Vector2[]>>();
        data.BaseDomain.PlayerAux.PlayerChangedRegime.Subscribe(a =>
        {
            var localPlayer = data.BaseDomain.PlayerAux.LocalPlayer;
            if (data.BaseDomain.PlayerAux.ByRegime.ContainsKey(a.NewVal))
            {
                var player = data.BaseDomain.PlayerAux.ByRegime[a.NewVal];
                if(player.PlayerGuid == localPlayer.PlayerGuid)
                {
                    ResetMajorOrders(data);
                    ResetMinorOrders(data);
                }
            }
        });
        data.Notices.Ticked.Subscribe(i => ResetTurnOrder(data));
    }

    public void SetLocalPlayerGuid(Guid guid)
    {
        if (LocalPlayerGuid != default)
        {
            throw new Exception();
        }

        LocalPlayerGuid = guid;
    }
    public void ResetTurnOrder(Data data)
    {
        var localPlayer = data.BaseDomain.PlayerAux.LocalPlayer;

        if (data.BaseDomain.GameClock.MajorTurn(data))
        {
            ResetMajorOrders(data);
        }
        else
        {
            ResetMinorOrders(data);
        }
    }

    public void ResetMajorOrders(Data data)
    {
        var localPlayer = data.BaseDomain.PlayerAux.LocalPlayer;
        MajorOrders = MajorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, localPlayer.Regime.Get(data));
    }
    public void ResetMinorOrders(Data data)
    {
        var localPlayer = data.BaseDomain.PlayerAux.LocalPlayer;
        MinorOrders = MinorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, localPlayer.Regime.Get(data));
    }

    public RegimeTurnOrders GetOrdersForThisTurn(Data data)
    {
        RegimeTurnOrders orders;
        if (data.BaseDomain.GameClock.MajorTurn(data))
        {
            orders = MajorOrders;
        }
        else
        {
            orders = MinorOrders;
        }

        return orders;
    }
}
