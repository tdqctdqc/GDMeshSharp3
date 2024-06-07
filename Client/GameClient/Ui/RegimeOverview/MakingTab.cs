using System;
using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class MakingTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    
    public MakingTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Making";
        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }

    private MakingTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
        _container.CreateLabelAsChild("MANUFACTURING QUEUE");

        var manufacturing = regime.MakeQueue.Queue;
        for (var i = 0; i < manufacturing.Count; i++)
        {
            var project = manufacturing[i];
            if(project is null) throw new Exception();
            _container.AddChild(project.GetDisplay(client.Data));
        }
    }
}