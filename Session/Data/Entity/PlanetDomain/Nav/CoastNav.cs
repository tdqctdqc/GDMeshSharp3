using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class CoastNav : LandNav
{
    public List<int> Seas { get; private set; }
    public static CoastNav Construct(List<int> seas)
    {
        return new CoastNav(seas);
    }
    [SerializationConstructor] private CoastNav(List<int> seas) : base(0f)
    {
        Seas = seas;
    }
}
