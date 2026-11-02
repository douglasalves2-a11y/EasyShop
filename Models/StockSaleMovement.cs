using LiteDB;

namespace EasyShop.Models;

public class StockSaleMovement
{
    [BsonId]
    public int SaleId { get; set; }

    public DateTime MovementDate { get; set; }

    public List<StockSaleItem> Items { get; set; } = new();
}

public class StockSaleItem
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
}