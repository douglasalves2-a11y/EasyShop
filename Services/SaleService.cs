using EasyShop.Data.Sql;
using EasyShop.Models;

namespace EasyShop.Services;

public class SaleService
{
    private readonly SaleRepository _saleRepository;
    private readonly CustomerService _customerService;
    private readonly ProductService _productService;

    public SaleService()
    {
        _saleRepository = new SaleRepository();
        _customerService = new CustomerService();
        _productService = new ProductService();
    }

    public async Task InitializeAsync()
    {
        await _customerService.InitializeAsync();
        await _saleRepository.InitializeAsync();

        await RecoverPendingSalesAsync();
    }

    public async Task<int> CreateSaleAsync(
        int customerId,
        List<SaleItem> requestedItems)
    {
        if (customerId <= 0)
            throw new ArgumentException("Selecione um cliente válido.");

        if (requestedItems is null || requestedItems.Count == 0)
            throw new ArgumentException("Adicione pelo menos um produto à venda.");

        var customer = await _customerService.GetCustomerByIdAsync(customerId);

        if (customer is null)
            throw new InvalidOperationException("Cliente não encontrado.");

        var groupedItems = requestedItems
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        var saleItems = new List<SaleItem>();

        foreach (var requestedItem in groupedItems)
        {
            if (requestedItem.ProductId <= 0)
                throw new ArgumentException("Produto inválido.");

            if (requestedItem.Quantity <= 0)
                throw new ArgumentException("Quantidade inválida.");

            var product = _productService.GetProductById(
                requestedItem.ProductId);

            if (product is null)
                throw new InvalidOperationException("Produto não encontrado.");

            if (requestedItem.Quantity > product.Stock)
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente para {product.Name}. " +
                    $"Disponível: {product.Stock}."
                );
            }

            saleItems.Add(new SaleItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = requestedItem.Quantity
            });
        }

        decimal total = saleItems.Sum(item => item.Subtotal);

        var sale = new Sale
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            SaleDate = DateTime.Now,
            Total = total,
            Status = "Pendente"
        };

        int saleId = await _saleRepository.AddSaleAsync(
            sale,
            saleItems);

        try
        {
            await ProcessPendingSaleAsync(saleId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"A venda #{saleId} foi registrada, mas permanece pendente. " +
                "O processamento será tentado novamente na próxima inicialização. " +
                $"Detalhes: {ex.Message}",
                ex
            );
        }

        return saleId;
    }

    private async Task ProcessPendingSaleAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);

        if (sale is null)
            throw new InvalidOperationException("Venda não encontrada.");

        if (sale.Status != "Pendente")
            return;

        var items = await _saleRepository.GetItemsBySaleIdAsync(saleId);

        if (items.Count == 0)
            throw new InvalidOperationException(
                "A venda não possui itens."
            );

        var movement = new StockSaleMovement
        {
            SaleId = saleId,
            Items = items.Select(item => new StockSaleItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity
            }).ToList()
        };

        // Se retornar false, a movimentação já existe.
        // Nesse caso, não haverá uma segunda baixa de estoque.
        _productService.ApplySaleStockMovement(movement);

        bool updated = await _saleRepository.UpdateStatusAsync(
            saleId,
            "Concluida");

        if (!updated)
        {
            throw new InvalidOperationException(
                "Não foi possível concluir a venda no SQLite."
            );
        }
    }

    private async Task RecoverPendingSalesAsync()
    {
        var pendingSales = await _saleRepository.GetPendingSalesAsync();

        foreach (var sale in pendingSales)
        {
            try
            {
                await ProcessPendingSaleAsync(sale.Id);
            }
            catch
            {
                // A venda continua pendente.
                // Uma nova tentativa será feita na próxima inicialização.
            }
        }
    }

    public async Task<List<Sale>> GetSalesAsync()
    {
        return await _saleRepository.GetAllAsync();
    }

    public async Task<List<SaleItem>> GetSaleItemsAsync(int saleId)
    {
        if (saleId <= 0)
            throw new ArgumentException("Informe uma venda válida.");

        return await _saleRepository.GetItemsBySaleIdAsync(saleId);
    }
}