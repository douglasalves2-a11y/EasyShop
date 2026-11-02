using SQLite;

namespace EasyShop.Models;

[Table("Sales")]
public class Sale
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CustomerId { get; set; }

    [NotNull]
    public string CustomerName { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public decimal Total { get; set; }

    [NotNull]
    public string Status { get; set; } = "Pendente";
}