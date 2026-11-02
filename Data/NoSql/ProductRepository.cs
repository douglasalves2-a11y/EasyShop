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

    public void ApplyStockChanges(Dictionary<int, int> stockChanges)
    {
        if (stockChanges is null || stockChanges.Count == 0)
            throw new ArgumentException("Nenhuma alteração de estoque informada.");

        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        database.BeginTrans();

        try
        {
            foreach (var change in stockChanges)
            {
                int productId = change.Key;
                int quantityChange = change.Value;

                var product = products.FindById(productId);

                if (product is null)
                    throw new InvalidOperationException(
                        $"Produto ID {productId} não encontrado."
                    );

                int newStock = product.Stock + quantityChange;

                if (newStock < 0)
                {
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para {product.Name}. " +
                        $"Disponível: {product.Stock}."
                    );
                }

                product.Stock = newStock;

                if (!products.Update(product))
                {
                    throw new InvalidOperationException(
                        $"Não foi possível atualizar o estoque de {product.Name}."
                    );
                }
            }

            database.Commit();
        }
        catch
        {
            database.Rollback();
            throw;
        }
    }

    public bool ApplySaleStockMovement(StockSaleMovement movement)
    {
        if (movement is null)
            throw new ArgumentNullException(nameof(movement));

        if (movement.SaleId <= 0)
            throw new ArgumentException("ID da venda inválido.");

        if (movement.Items is null || movement.Items.Count == 0)
            throw new ArgumentException("A venda não possui itens.");

        using var database = new LiteDatabase(_databasePath);

        var products = database.GetCollection<Product>("products");

        var movements = database.GetCollection<StockSaleMovement>(
            "stock_sale_movements"
        );

        database.BeginTrans();

        try
        {
            // Evita descontar o estoque duas vezes na mesma venda.
            var existingMovement = movements.FindById(movement.SaleId);

            if (existingMovement is not null)
            {
                database.Commit();
                return false;
            }

            // Agrupa produtos repetidos.
            var groupedItems = movement.Items
                .GroupBy(item => item.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                })
                .ToList();

            var savedItems = new List<StockSaleItem>();

            foreach (var item in groupedItems)
            {
                if (item.ProductId <= 0 || item.Quantity <= 0)
                    throw new InvalidOperationException("Item da venda inválido.");

                var product = products.FindById(item.ProductId);

                if (product is null)
                    throw new InvalidOperationException("Produto não encontrado.");

                if (product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para {product.Name}. " +
                        $"Disponível: {product.Stock}."
                    );
                }

                product.Stock -= item.Quantity;

                if (!products.Update(product))
                {
                    throw new InvalidOperationException(
                        $"Não foi possível atualizar o estoque de {product.Name}."
                    );
                }

                savedItems.Add(new StockSaleItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = item.Quantity
                });
            }

            movement.Items = savedItems;
            movement.MovementDate = DateTime.Now;

            movements.Insert(movement);

            database.Commit();

            return true;
        }
        catch
        {
            database.Rollback();
            throw;
        }
    }
}