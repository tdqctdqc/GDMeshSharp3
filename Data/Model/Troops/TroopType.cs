
using Godot;

public class TroopType : IModel, IIconed
{
    public int Id { get; private set; }
    public int Echelon { get; private set; }
    public string Name { get; private set; }
    public float FrontLength { get; private set; }
    public TroopDomain TroopDomain { get; private set; }
    public Icon Icon { get; private set; }
    
    public void CreateIcon()
    {
        Icon = Icon.Create(Name, Vector2I.One);
    }
}