using EasyShop.Data.NoSql;
using EasyShop.Models;

namespace EasyShop.Services;

public class ProductService
{
    private readonly ProductRepository _repository;

    public ProductService()
    {
        _repository = new ProductRepository();
    }

    public List<Product> GetProducts()
    {
        return _repository.GetAll();
    }

    public Product? GetProductById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("O ID do produto deve ser válido.");

        return _repository.GetById(id);
    }

    public int AddProduct(Product product)
    {
        ValidateProduct(product);

        product.Name = product.Name.Trim();
        product.Description = product.Description.Trim();
        product.Category = product.Category.Trim();

        product.Tags = product.Tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return _repository.Add(product);
    }

    public bool UpdateProduct(Product product)
    {
        ValidateProduct(product);

        if (product.Id <= 0)
            throw new ArgumentException("O ID do produto deve ser válido.");

        product.Name = product.Name.Trim();
        product.Description = product.Description.Trim();
        product.Category = product.Category.Trim();

        product.Tags = product.Tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return _repository.Update(product);
    }

    public bool DeleteProduct(int id)
    {
        if (id <= 0)
            throw new ArgumentException("O ID do produto deve ser válido.");

        return _repository.Delete(id);
    }

    private static void ValidateProduct(Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Informe o nome do produto.");

        if (string.IsNullOrWhiteSpace(product.Category))
            throw new ArgumentException("Informe a categoria do produto.");

        if (product.Price <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.");

        if (product.Stock < 0)
            throw new ArgumentException("O estoque não pode ser negativo.");

        if (product.Tags is null)
            product.Tags = new List<string>();
    }
}