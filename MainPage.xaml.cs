using EasyShop.Views;

namespace EasyShop;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCustomersClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CustomersPage));
    }

    private async void OnProductsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProductsPage));
    }

    private async void OnSalesClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SalesPage));
    }
}