using EasyShop.Models;
using SQLite;

namespace EasyShop.Data.Sql;

public class CustomerRepository
{
    private readonly SQLiteAsyncConnection _database;

    public CustomerRepository()
    {
        string databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "easyshop.db3"
        );

        _database = new SQLiteAsyncConnection(databasePath);
    }

    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Customer>();
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _database.Table<Customer>()
            .OrderBy(customer => customer.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _database.Table<Customer>()
            .Where(customer => customer.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> AddAsync(Customer customer)
    {
        return await _database.InsertAsync(customer);
    }

    public async Task<int> UpdateAsync(Customer customer)
    {
        return await _database.UpdateAsync(customer);
    }

    public async Task<int> DeleteAsync(Customer customer)
    {
        return await _database.DeleteAsync(customer);
    }
}