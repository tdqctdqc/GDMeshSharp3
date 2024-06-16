
using System.Linq;
using Godot;

public partial class ArmyHistoryGraphic : Node2D
{
    private MeshInstance2D _mesh;
    public ArmyHistoryGraphic()
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.ArmyHistory;
        _mesh = new MeshInstance2D();
        AddChild(_mesh);
    }
    public void Draw(Army army, Client client)
    {
        var segmenter = client.GetComponent<MapGraphics>().Segmenter;
        var relTo = army.GetHomeCell(client.Data).RelTo;
        client.QueuedUpdates.Enqueue(() => segmenter.AddElement(this, relTo));

        var tick = client.Data.BaseDomain.GameClock.Tick;
        var histories = client.Data.Military.CombatHistories.Value;
        var mb = new MeshBuilder();
        if (histories.Histories.TryGetValue(tick, out var history))
        {
            if (history.ArmyCombatHistories.TryGetValue(army.MakeRef(), out var armyHistory))
            {
                drawHist(history, armyHistory);
            }
        }
        else if (histories.Histories.TryGetValue(tick - 1, out var prevHistory))
        {
            if (prevHistory.ArmyCombatHistories.TryGetValue(army.MakeRef(), out var prevArmyHistory))
            {
                drawHist(prevHistory, prevArmyHistory);
            }
        }
        else
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                _mesh.Mesh = null;
            });
            return;
        }
        
        client.QueuedUpdates.Enqueue(() =>
        {
            _mesh.Mesh = mb.GetMesh();
        });
        void drawHist(CombatHistory history, ArmyCombatHistory armyHistory)
        {
            foreach (var cellRef in armyHistory.Attacked)
            {
                var cell = cellRef.Get(client.Data);
                var attackedFrom = armyHistory.Occupied
                    .Where(c => cell.Neighbors.Contains(c.RefId))
                    .Select(c => c.Get(client.Data));
                foreach (var from in attackedFrom)
                {
                    mb.AddArrowRel(from.GetCenter(), 
                        cell.GetCenter(), 
                        3f, Colors.Blue, relTo, client.Data);
                }
            }

            foreach (var cellRef in armyHistory.Defended)
            {
                var cell = cellRef.Get(client.Data);
                // var attackedFrom = history.CellCombatHistories[cellRef]
                //     .
            }
        }
    }
}