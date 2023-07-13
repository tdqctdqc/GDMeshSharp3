using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeDiplomacy
{
    public static RegimeDiplomacy Construct(Data data)
    {
        return new RegimeDiplomacy();
    }
    [SerializationConstructor] private RegimeDiplomacy()
    {
    }
}
