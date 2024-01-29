using Godot;

public class PreGraphFailureException : DisplayableException
{
    public Graph<MapPolygon, LineSegment> Graph { get; private set; }

    public PreGraphFailureException(Graph<MapPolygon, LineSegment> graph)
    {
        Graph = graph;
    }

    public override Node2D GetGraphic()
    {
        var d = SceneManager.Instance<PreGraphFailureDisplay>();
        d.Setup(this);
        return d;
    }

    public override Control GetUi()
    {
        throw new System.NotImplementedException();
    }
}
