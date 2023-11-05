
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public class OrderHolder
{
    public ConcurrentDictionary<Regime, RegimeTurnOrders> Orders { get; private set; }
    private ConcurrentDictionary<Regime, Task<RegimeTurnOrders>> _aiOrderCalcs;
    private ConcurrentDictionary<Regime, CancellationTokenSource> _aiCalcCancelTokens;
    private LogicWriteKey _key;
    public OrderHolder(LogicWriteKey key)
    {
        _key = key;
        Orders = new ConcurrentDictionary<Regime, RegimeTurnOrders>();
        _aiCalcCancelTokens = new ConcurrentDictionary<Regime, CancellationTokenSource>();
        _aiOrderCalcs = new ConcurrentDictionary<Regime, Task<RegimeTurnOrders>>();
        key.Data.BaseDomain.PlayerAux.PlayerChangedRegime
            .Subscribe(HandlePlayerChangedRegime);
        key.Data.Requests.SubmitPlayerOrders
            .Subscribe(x => SubmitPlayerTurnOrders(x.Item1, x.Item2, _key.Data));
    }

    public void HandlePlayerChangedRegime(ValChangeNotice<Player, Regime> notice)
    {
        CancelCalcAiRegimeOrders(notice.NewVal, _key);
        CalcAiRegimeOrders(notice.OldVal, _key);
    }
    public void SubmitPlayerTurnOrders(Player player, RegimeTurnOrders orders, Data data)
    {
        if (orders.Tick != data.BaseDomain.GameClock.Tick) throw new Exception();
        var regime = orders.Regime.Entity(data);
        if (Orders.ContainsKey(regime) && Orders[regime] != null) throw new Exception();
        Orders[regime] = orders;
    }
    public void CalcAiOrders(LogicWriteKey key)
    {
        var aiRegimes = key.Data.GetAll<Regime>()
            .Where(r => r.IsPlayerRegime(key.Data) == false);
        foreach (var r in aiRegimes)
        {
            CalcAiRegimeOrders(r, key);
        }
    }
    public void Clear()
    {
        foreach (var r in Orders.Keys.ToList())
        {
            Orders[r] = null;
            CancelCalcAiRegimeOrders(r, _key);
        }
    }
    public List<RegimeTurnOrders> GetOrdersList(Data data)
    {
        return Orders.Values.ToList();
    }
    private async void CalcAiRegimeOrders(Regime r, LogicWriteKey key)
    {
        if (r == null) return;
        Orders[r] = null;
        var source = new CancellationTokenSource();
        _aiCalcCancelTokens[r] = source;
        Func<RegimeTurnOrders> func = () =>
        {
            var ai = key.Data.HostLogicData.RegimeAis[r];
            var orders = (RegimeTurnOrders)ai.CalculateAndSendOrders(key);
            Orders[r] = orders;
            return orders;
        };
        var task = Task.Run(func, source.Token);
        
        _aiOrderCalcs.TryAdd(r, task);

        try
        {
            await task;
        }
        catch
        {
            throw;
        }
    }

    private void CancelCalcAiRegimeOrders(Regime r, LogicWriteKey key)
    {
        if (r == null) return;
        if (_aiOrderCalcs.ContainsKey(r) == false) return;
        _aiCalcCancelTokens[r]?.Cancel();
        _aiCalcCancelTokens[r] = null;
        _aiOrderCalcs[r] = null;
        Orders[r] = null;
    }
    

    
    public bool CheckReadyForFrame(Data data, bool majorTurn)
    {
        var regimes = data.GetAll<Regime>();
        return regimes.All(r => Orders.ContainsKey(r) && Orders[r] != null);
    }
    
    public Vector2 GetNumAisReady(Data data)
    {
        var ais = Orders.Where(kvp => kvp.Key.IsPlayerRegime(data) == false);
        var readyAis = ais.Where(kvp => kvp.Value != null);
        return new Vector2(readyAis.Count(), ais.Count());
    }
    
    public Vector2 GetNumPlayersReady(Data data)
    {
        var players = Orders.Where(kvp => kvp.Key.IsPlayerRegime(data));
        var readyPlayers = players.Where(kvp => kvp.Value != null);
        return new Vector2(readyPlayers.Count(), players.Count());
    }
}