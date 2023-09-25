
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Item : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public AttributeHolder<IItemAttribute> Attributes { get; private set; }  
    
    protected Item(string name, Color color, 
        params IItemAttribute[] attributes)
    {
        Attributes = new AttributeHolder<IItemAttribute>(attributes);
        Name = name;
        Color = color;
        Icon = Icon.Create(Name, Icon.AspectRatio._1x1, 50f);
    }
}
