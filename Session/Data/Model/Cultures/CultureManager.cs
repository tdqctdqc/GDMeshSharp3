using System;
using System.Collections.Generic;
using System.Linq;

public class CultureManager : IModelManager<Culture>
{
    public Dictionary<string, Culture> Models { get; private set; }
    
    public CultureManager()
    {
        Models = CultureLoader.Setup();
    }
}
