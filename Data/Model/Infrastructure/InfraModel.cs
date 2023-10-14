using System.Collections.Generic;

public class InfraModel : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    
    public Icon Icon { get; }

    public InfraModel(string name)
    {
        Name = name;
        Icon = Icon.Create(Name, Icon.AspectRatio._1x1, 25f);
    }
}