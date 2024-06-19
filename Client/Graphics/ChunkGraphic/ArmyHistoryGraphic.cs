
using System.Linq;
using Godot;

public partial class ArmyHistoryGraphic : Node2D
{
    private MeshInstance2D _mesh;
    private Control _controls;
    public ArmyHistoryGraphic()
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.ArmyHistory;
    }

    public void Initialize()
    {
        _mesh = new MeshInstance2D();
        AddChild(_mesh);
        _controls = new Control();
        AddChild(_controls);
        _controls.MouseFilter = Control.MouseFilterEnum.Pass;
    }
    public void Draw(Army army, Client client)
    {
        var segmenter = client.GetComponent<MapGraphics>().Segmenter;
        var relTo = army.GetHomeCell(client.Data).RelTo;
        client.QueuedUpdates.Enqueue(
            () => segmenter.AddElement(this, relTo));

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

        if (mb.TriVertices.Count == 0)
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
        
        var vertices = mb.TriVertices.ToArray();
        client.QueuedUpdates.Enqueue(() =>
        {
            var c = CustomArea.Construct(_mesh, relTo, vertices,
                m =>
                {
                    
                } );
            c.ZIndex = (int)LayerOrder.Ui;
            c.ZAsRelative = false;
            _controls.ClearChildren();
            _controls.AddChild(c);
        });
        
        
        
        
        void drawHist(CombatHistory history, ArmyCombatHistory armyHistory)
        {
            foreach (var cellRef in armyHistory.Attacked)
            {
                var cell = cellRef.Get(client.Data);
                var cellHist = history.CellCombatHistories[cellRef];
                var attackedFrom = armyHistory.Occupied
                    .Where(c => cell.Neighbors.Contains(c.RefId))
                    .Select(c => c.Get(client.Data));
                foreach (var from in attackedFrom)
                {
                    if (cellHist.ForcedBack)
                    {
                        mb.AddArrowRel(from.GetCenter(), 
                            cell.GetCenter(), 
                            3f, Colors.Green, relTo, client.Data);
                    }
                    else
                    {
                        var e = from.GetEdgeRelWith(cell);
                        var mid = (e.Item1 + e.Item2) / 2f + from.GetCenter();
                        mb.AddArrowRel(from.GetCenter(), 
                            mid, 
                            3f, Colors.Blue, relTo, client.Data);
                    }
                }
            }

            foreach (var cellRef in armyHistory.Defended)
            {
                var cell = cellRef.Get(client.Data);
                var cellHist = history.CellCombatHistories[cellRef];
                var color = cellHist.ForcedBack ? Colors.Red : Colors.Orange;
                mb.AddSquare(relTo.Offset(cell.GetCenter(), client.Data),
                    10f, color);
            }
        }
    }



    private void Open()
    {
        
    }
}