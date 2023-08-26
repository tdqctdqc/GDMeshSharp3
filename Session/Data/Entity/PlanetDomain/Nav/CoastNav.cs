using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class CoastNav : LandNav
{
    public int Sea { get; private set; }
    public bool Port { get; private set; }
    public static CoastNav Construct(int sea)
    {
        return new CoastNav(sea, false);
    }
    [SerializationConstructor] private CoastNav(int sea, bool port) 
        : base(0f)
    {
        Port = port;
        Sea = sea;
    }

    public void SetPort(bool port, GenWriteKey key)
    {
        Port = port;
    }
}
