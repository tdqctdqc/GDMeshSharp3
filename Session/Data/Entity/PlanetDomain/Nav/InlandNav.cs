using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class InlandNav : LandNav
{
    public static InlandNav Construct(params MapPolygon[] polys)
    {
        return null;
    }

    public InlandNav() : base(0f)
    {
        
    }
    [SerializationConstructor] private InlandNav(float roughness) : base(roughness)
    {
    }
}
