
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Item : IModel, IIconed
{
    public string Name { get; protected set; }
    public int Id { get; private set; }
    public Icon Icon { get; private set; }
    
    protected Item()
    {
        
    }

    public void CreateIcon()
    {
        Icon = Icon.Create(Name, Vector2I.One);
    }
}
