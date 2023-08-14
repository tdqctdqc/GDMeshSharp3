using MessagePack;

public class RegimeItemTradeReport
{
    public int ItemId { get; set; }
    public int RegimeId { get; set; }
    public int QuantityDemanded { get; set; }
    public int QuantityOffered { get; set; }
    public int QuantityBought { get; set; }
    public int QuantitySold { get; set; }
    
    public static RegimeItemTradeReport Construct(int itemId, int regimeId)
    {
        return new RegimeItemTradeReport(itemId, regimeId, 0, 0,0,0);
    }
    [SerializationConstructor] private RegimeItemTradeReport(int itemId, int regimeId, int quantityDemanded, int quantityOffered, int quantityBought, int quantitySold)
    {
        ItemId = itemId;
        RegimeId = regimeId;
        QuantityDemanded = quantityDemanded;
        QuantityOffered = quantityOffered;
        QuantityBought = quantityBought;
        QuantitySold = quantitySold;
    }

    public int Net()
    {
        return QuantityBought - QuantitySold;
    }
}