using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeItemReport
{
    public float Produced { get; set; } = 0f;
    public float Consumed { get; set; } = 0f;
    public float Demanded { get; set; } = 0f;
    public float Offered { get; set; } = 0f;
    public float Bought { get; set; } = 0f;
    public float Sold { get; set; } = 0f;

    public static RegimeItemReport Construct()
    {
        return new RegimeItemReport(0, 0, 0, 0, 0, 0);
    }
    [SerializationConstructor] private RegimeItemReport(float produced, float demanded, float offered, float bought, float sold, float consumed)
    {
        Produced = produced;
        Demanded = demanded;
        Offered = offered;
        Bought = bought;
        Sold = sold;
        Consumed = consumed;
    }
}
