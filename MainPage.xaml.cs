namespace EasyShop;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCustomersClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Clientes",
            "A tela de cadastro de clientes será implementada na próxima etapa.",
            "OK"
        );
    }

    private async void OnProductsClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Produtos",
            "A tela do catálogo de produtos será implementada na próxima etapa.",
            "OK"
        );
    }
}
