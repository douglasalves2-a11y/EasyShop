using EasyShop.Models;
using SQLite;

namespace EasyShop.Data.Sql;

public class SaleRepository
{
    private readonly SQLiteAsyncConnection _database;

    public SaleRepository()
    {
        string databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "easyshop.db3"
        );

        _database = new SQLiteAsyncConnection(databasePath);
    }

    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Sale>();
        await _database.CreateTableAsync<SaleItem>();
    }

    public async Task<int> AddSaleAsync(Sale sale, List<SaleItem> items)
    {
        if (sale is null)
            throw new ArgumentNullException(nameof(sale));

        if (items is null || items.Count == 0)
            throw new ArgumentException("A venda precisa ter pelo menos um produto.");

        await _database.RunInTransactionAsync(connection =>
        {
            connection.Insert(sale);

            foreach (var item in items)
            {
                item.SaleId = sale.Id;
                connection.Insert(item);
            }
        });

        return sale.Id;
    }

    public async Task<List<Sale>> GetAllAsync()
    {
        return await _database.Table<Sale>()
            .OrderByDescending(sale => sale.SaleDate)
            .ToListAsync();
    }

    public async Task<Sale?> GetByIdAsync(int id)
    {
        return await _database.Table<Sale>()
            .Where(sale => sale.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SaleItem>> GetItemsBySaleIdAsync(int saleId)
    {
        return await _database.Table<SaleItem>()
            .Where(item => item.SaleId == saleId)
            .ToListAsync();
    }
public async Task<bool> UpdateStatusAsync(int saleId, string status)
{
    var sale = await GetByIdAsync(saleId);

    if (sale is null)
        return false;

    sale.Status = status;

    int result = await _database.UpdateAsync(sale);

    return result > 0;
}
public async Task<List<Sale>> GetPendingSalesAsync()
{
    return await _database.Table<Sale>()
        .Where(sale => sale.Status == "Pendente")
        .ToListAsync();
}
}