using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemWallet : Wallet<int>
{
    public int this[Item item] => this[item.Id];
    public static ItemWallet Construct()
    {
        return new ItemWallet(new Dictionary<int, int>());
    }
    public static ItemWallet Construct(ItemWallet toCopy)
    {
        return new ItemWallet(new Dictionary<int, int>(toCopy.Contents));
    }
    [SerializationConstructor] private ItemWallet(Dictionary<int, int> contents) : base(contents)
    {
    }

    public void Add(Item item, int amount)
    {
        Add(item.Id, amount);
    }
    public void Remove(Item item, int amount)
    {
        Remove(item.Id, amount);
    }
}
