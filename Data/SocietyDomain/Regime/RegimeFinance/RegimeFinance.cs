using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeFinance 
{
    public float LastTradeBalance { get; private set; }
    public static RegimeFinance Construct()
    {
        return new RegimeFinance(0);
    }
    [SerializationConstructor] private RegimeFinance(float lastTradeBalance)
    {
        LastTradeBalance = lastTradeBalance;
    }

    public void AddToTradeBalance(float balance, ProcedureWriteKey key)
    {
        LastTradeBalance += balance;
    }

    public void ClearTradeBalance(ProcedureWriteKey key)
    {
        LastTradeBalance = 0;
    }
}
