using EasyShop.Views;

namespace EasyShop;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(CustomersPage),
            typeof(CustomersPage));

        Routing.RegisterRoute(
            nameof(ProductsPage),
            typeof(ProductsPage));

        Routing.RegisterRoute(
            nameof(SalesPage),
            typeof(SalesPage));
    }
}