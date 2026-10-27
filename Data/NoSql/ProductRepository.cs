using EasyShop.Models;
using LiteDB;

namespace EasyShop.Data.NoSql;

public class ProductRepository
{
    private readonly string _databasePath;

    public ProductRepository()
    {
        _databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "easyshop_products.db"
        );
    }

    public int Add(Product product)
    {
        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        products.EnsureIndex(product => product.Name);

        products.Insert(product);

        return product.Id;
    }

    public List<Product> GetAll()
    {
        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        return products.FindAll().ToList();
    }

    public Product? GetById(int id)
    {
        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        return products.FindById(id);
    }

    public bool Update(Product product)
    {
        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        return products.Update(product);
    }

    public bool Delete(int id)
    {
        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        return products.Delete(id);
    }
}