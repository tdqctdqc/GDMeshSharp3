
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class OrderHolder
{
    public ConcurrentDictionary<Regime, RegimeTurnOrders> Orders { get; private set; }
    private ConcurrentDictionary<Regime, Task<RegimeTurnOrders>> _aiOrderCalcs;
    private ConcurrentDictionary<Regime, CancellationTokenSource> _aiCalcCancelTokens;
    private Data _data;
    public OrderHolder(Data data)
    {
        _data = data;
        Orders = new ConcurrentDictionary<Regime, RegimeTurnOrders>();
        _aiCalcCancelTokens = new ConcurrentDictionary<Regime, CancellationTokenSource>();
        _aiOrderCalcs = new ConcurrentDictionary<Regime, Task<RegimeTurnOrders>>();
        data.BaseDomain.PlayerAux.PlayerChangedRegime
            .Subscribe(HandlePlayerChangedRegime);
        data.Requests.SubmitPlayerOrders
            .Subscribe(x => SubmitPlayerTurnOrders(x.Item1, x.Item2, _data));
    }

    public void HandlePlayerChangedRegime(ValChangeNotice<Player, Regime> notice)
    {
        CancelCalcAiRegimeOrders(notice.NewVal, _data);
        CalcAiRegimeOrders(notice.OldVal, _data);
    }
    public void SubmitPlayerTurnOrders(Player player, RegimeTurnOrders orders, Data data)
    {
        if (orders.Tick != data.BaseDomain.GameClock.Tick) throw new Exception();
        var regime = orders.Regime.Entity(data);
        if (Orders.ContainsKey(regime) && Orders[regime] != null) throw new Exception();
        Orders[regime] = orders;
    }
    public void CalcAiOrders(Data data)
    {
        var aiRegimes = data.GetAll<Regime>()
            .Where(r => r.IsPlayerRegime(data) == false);
        foreach (var r in aiRegimes)
        {
            CalcAiRegimeOrders(r, data);
        }
    }

    public void Clear()
    {
        foreach (var r in Orders.Keys.ToList())
        {
            Orders[r] = null;
            CancelCalcAiRegimeOrders(r, _data);
        }
    }
    public List<RegimeTurnOrders> GetOrdersList(Data data)
    {
        return Orders.Values.ToList();
    }
    private async void CalcAiRegimeOrders(Regime r, Data data)
    {
        Orders[r] = null;
        var source = new CancellationTokenSource();
        _aiCalcCancelTokens[r] = source;
        Func<RegimeTurnOrders> func = () =>
        {
            var orders = (RegimeTurnOrders)data.HostLogicData.RegimeAis[r].GetTurnOrders(data);
            Orders[r] = orders;
            return orders;
        };
        _aiOrderCalcs.TryAdd(r, Task.Run(func, source.Token));
    }

    private void CancelCalcAiRegimeOrders(Regime r, Data data)
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
}