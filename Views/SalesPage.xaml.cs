using System.Globalization;
using EasyShop.Models;
using EasyShop.Services;

namespace EasyShop.Views;

public partial class SalesPage : ContentPage
{
    private readonly SaleService _saleService;
    private readonly CustomerService _customerService;
    private readonly ProductService _productService;

    private readonly List<SaleItem> _cart = new();

    private List<Product> _products = new();

    private static readonly CultureInfo PtBr =
        CultureInfo.GetCultureInfo("pt-BR");

    public SalesPage()
    {
        InitializeComponent();

        _saleService = new SaleService();
        _customerService = new CustomerService();
        _productService = new ProductService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _saleService.InitializeAsync();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async Task LoadDataAsync()
    {
        var customers = await _customerService.GetCustomersAsync();

        _products = _productService.GetProducts();

        CustomerPicker.ItemsSource = customers;
        ProductPicker.ItemsSource = _products;

        await LoadSalesHistoryAsync();
    }

    private void OnAddProductClicked(object? sender, EventArgs e)
    {
        if (ProductPicker.SelectedItem is not Product product)
        {
            _ = DisplayAlertAsync(
                "Atenção",
                "Selecione um produto.",
                "OK");
            return;
        }

        if (!int.TryParse(QuantityEntry.Text, out int quantity) ||
            quantity <= 0)
        {
            _ = DisplayAlertAsync(
                "Atenção",
                "Informe uma quantidade válida.",
                "OK");
            return;
        }

        int quantityAlreadyAdded = _cart
            .Where(item => item.ProductId == product.Id)
            .Sum(item => item.Quantity);

        if (quantity + quantityAlreadyAdded > product.Stock)
        {
            _ = DisplayAlertAsync(
                "Estoque insuficiente",
                $"Disponível: {product.Stock} unidades.\n" +
                $"Já adicionadas: {quantityAlreadyAdded}.",
                "OK");
            return;
        }

        var existingItem = _cart.FirstOrDefault(
            item => item.ProductId == product.Id);

        if (existingItem is not null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _cart.Add(new SaleItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }

        QuantityEntry.Text = string.Empty;
        ProductPicker.SelectedItem = null;

        RenderCart();
    }

    private void RenderCart()
    {
        SaleItemsList.Children.Clear();

        foreach (var item in _cart)
        {
            var content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label
                    {
                        Text = item.ProductName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = Color.FromArgb("#183153")
                    },
                    new Label
                    {
                        Text = $"Quantidade: {item.Quantity}",
                        TextColor = Color.FromArgb("#5D6D7E")
                    },
                    new Label
                    {
                        Text = $"Subtotal: {item.Subtotal.ToString("C", PtBr)}",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#16865B")
                    }
                }
            };

            var removeButton = new Button
            {
                Text = "Remover",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                CornerRadius = 8,
                BindingContext = item
            };

            removeButton.Clicked += OnRemoveItemClicked;

            content.Children.Add(removeButton);

            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#DCE3EB"),
                StrokeThickness = 1,
                Padding = 12,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 10
                },
                Content = content
            };

            SaleItemsList.Children.Add(card);
        }

        decimal total = _cart.Sum(item => item.Subtotal);

        TotalLabel.Text = $"Total: {total.ToString("C", PtBr)}";
    }

    private void OnRemoveItemClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not SaleItem item)
            return;

        _cart.Remove(item);

        RenderCart();
    }

    private async void OnFinishSaleClicked(object? sender, EventArgs e)
    {
        if (CustomerPicker.SelectedItem is not Customer customer)
        {
            await DisplayAlertAsync(
                "Atenção",
                "Selecione um cliente.",
                "OK");
            return;
        }

        if (_cart.Count == 0)
        {
            await DisplayAlertAsync(
                "Atenção",
                "Adicione pelo menos um produto à venda.",
                "OK");
            return;
        }

        bool confirm = await DisplayAlertAsync(
            "Confirmar venda",
            $"Cliente: {customer.Name}\n" +
            $"Total: {_cart.Sum(item => item.Subtotal).ToString("C", PtBr)}\n\n" +
            "Deseja finalizar a venda?",
            "Sim",
            "Não");

        if (!confirm)
            return;

        try
        {
            int saleId = await _saleService.CreateSaleAsync(
                customer.Id,
                _cart.ToList());

            _cart.Clear();

            CustomerPicker.SelectedItem = null;

            RenderCart();

            await LoadSalesHistoryAsync();

            await DisplayAlertAsync(
                "Sucesso",
                $"Venda #{saleId} registrada com sucesso!",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        try
        {
            await LoadDataAsync();

            await DisplayAlertAsync(
                "Atualizado",
                "Clientes, produtos e vendas atualizados.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async Task LoadSalesHistoryAsync()
    {
        var sales = await _saleService.GetSalesAsync();

        SalesHistoryList.Children.Clear();

        foreach (var sale in sales)
        {
            var content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label
                    {
                        Text = $"Venda #{sale.Id}",
                        FontSize = 17,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#183153")
                    },
                    new Label
                    {
                        Text = $"Cliente: {sale.CustomerName}",
                        TextColor = Color.FromArgb("#5D6D7E")
                    },
                    new Label
                    {
                        Text = $"Data: {sale.SaleDate:dd/MM/yyyy HH:mm}",
                        TextColor = Color.FromArgb("#5D6D7E")
                    },
                    new Label
                    {
                        Text = $"Total: {sale.Total.ToString("C", PtBr)}",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#16865B")
                    }
                }
            };

            var detailsButton = new Button
            {
                Text = "Ver itens",
                BackgroundColor = Color.FromArgb("#2463EB"),
                TextColor = Colors.White,
                CornerRadius = 8,
                BindingContext = sale
            };

            detailsButton.Clicked += OnViewSaleClicked;

            content.Children.Add(detailsButton);

            SalesHistoryList.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#DCE3EB"),
                StrokeThickness = 1,
                Padding = 14,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 10
                },
                Content = content
            });
        }
    }

    private async void OnViewSaleClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not Sale sale)
            return;

        try
        {
            var items = await _saleService.GetSaleItemsAsync(sale.Id);

            string details = string.Join(
                "\n",
                items.Select(item =>
                    $"{item.ProductName} - {item.Quantity} x " +
                    $"{item.UnitPrice.ToString("C", PtBr)} = " +
                    $"{item.Subtotal.ToString("C", PtBr)}"));

            await DisplayAlertAsync(
                $"Venda #{sale.Id}",
                details,
                "Fechar");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
}