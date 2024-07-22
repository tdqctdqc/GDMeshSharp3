
public class TechnologyCategories : ModelPredefs<TechnologyCategory>
{
    public TechnologyCategory Economic { get; private set; }
        = new();
    public TechnologyCategory Military { get; private set; }
        = new();
    public TechnologyCategory Administrative { get; private set; }
        = new();
}