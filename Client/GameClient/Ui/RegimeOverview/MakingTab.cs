using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class MakingTab : ScrollContainer
{
    private VBoxContainer _container;
    public MakingTab()
    {
        Name = "Making";
        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        _container.CreateLabelAsChild("MANUFACTURING QUEUE");

        var manufacturing = regime.MakeQueue.Queue;
        for (var i = 0; i < manufacturing.Count; i++)
        {
            var project = manufacturing.ElementAt(i);
            _container.AddChild(project.GetDisplay(client.Data));
        }
    }
}