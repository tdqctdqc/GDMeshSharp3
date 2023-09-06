using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class ManufacturingTab : ScrollContainer
{
    private VBoxContainer _container;
    public ManufacturingTab()
    {
        Name = "Manufacturing";
        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        _container.CreateLabelAsChild("MANUFACTURING QUEUE");

        var manufacturing = regime.ManufacturingQueue.Queue;
        for (var i = 0; i < manufacturing.Count; i++)
        {
            var project = manufacturing.ElementAt(i).Value();
            _container.AddChild(project.GetDisplay(data));
        }
    }
}