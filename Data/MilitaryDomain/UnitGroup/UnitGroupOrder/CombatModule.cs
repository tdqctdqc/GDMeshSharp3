
using System.Collections.Generic;
using System.Linq;

public class CombatModule : LogicModule
{
    private Dictionary<Regime, MilAiMemo> _memos;
    private Data _data;
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        _data = key.Data;
        key.Data.Notices.CellChangedController.Subscribe(HandleCellChanged);
        _memos = key.Data.HostLogicData.RegimeAis.Dic
            .ToDictionary(kvp => kvp.Key,
                kvp => new MilAiMemo(kvp.Key, key.Data));
        new CombatCalculator().Calculate(key);
        key.Data.Notices.CellChangedController.Unsubscribe(HandleCellChanged);
        foreach (var milAiMemo in _memos.Values)
        {
            milAiMemo.Finish(key);
        }
        _memos.Clear();
    }

    private void HandleCellChanged((PolyCell c, Regime oldR, Regime newR) v)
    {
        if (_memos.ContainsKey(v.oldR) == false
            && _memos.ContainsKey(v.newR) == false)
        {
            return;
        }

        if (_memos.ContainsKey(v.oldR))
        {
            _memos[v.oldR].RemoveCell(v.c, _data);
        }
        if (_memos.ContainsKey(v.newR))
        {
            _memos[v.oldR].AddCell(v.c, _data);
        }
    }
}