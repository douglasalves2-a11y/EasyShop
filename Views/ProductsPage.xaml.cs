using System.Globalization;
using EasyShop.Models;
using EasyShop.Services;

namespace EasyShop.Views;

public partial class ProductsPage : ContentPage
{
    private readonly ProductService _productService;
    private int? _editingProductId;

    public ProductsPage()
    {
        InitializeComponent();
        _productService = new ProductService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            LoadProducts();
        }
        catch (Exception ex)
        {
            _ = DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void OnSaveProductClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!decimal.TryParse(
                    PriceEntry.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal price))
            {
                await DisplayAlertAsync(
                    "Atenção",
                    "Informe um preço válido.",
                    "OK");
                return;
            }

            if (!int.TryParse(StockEntry.Text, out int stock))
            {
                await DisplayAlertAsync(
                    "Atenção",
                    "Informe uma quantidade válida para o estoque.",
                    "OK");
                return;
            }

            var product = new Product
            {
                Name = NameEntry.Text ?? string.Empty,
                Description = DescriptionEntry.Text ?? string.Empty,
                Category = CategoryEntry.Text ?? string.Empty,
                Price = price,
                Stock = stock,
                Tags = (TagsEntry.Text ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => tag.Trim())
                    .ToList()
            };

            if (_editingProductId.HasValue)
            {
                product.Id = _editingProductId.Value;

                if (!_productService.UpdateProduct(product))
                {
                    await DisplayAlertAsync(
                        "Erro",
                        "Não foi possível atualizar o produto.",
                        "OK");
                    return;
                }

                ResetForm();
                LoadProducts();

                await DisplayAlertAsync(
                    "Sucesso",
                    "Produto atualizado com sucesso!",
                    "OK");
            }
            else
            {
                _productService.AddProduct(product);

                ResetForm();
                LoadProducts();

                await DisplayAlertAsync(
                    "Sucesso",
                    "Produto cadastrado com sucesso!",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void OnEditProductClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not Product product)
            return;

        _editingProductId = product.Id;

        NameEntry.Text = product.Name;
        DescriptionEntry.Text = product.Description;
        CategoryEntry.Text = product.Category;
        PriceEntry.Text = product.Price.ToString("0.00", CultureInfo.CurrentCulture);
        StockEntry.Text = product.Stock.ToString();
        TagsEntry.Text = string.Join(", ", product.Tags);

        SaveProductButton.Text = "Salvar alterações";
        CancelEditButton.IsVisible = true;

        _ = NameEntry.Focus();
    }

    private async void OnDeleteProductClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not Product product)
            return;

        bool confirm = await DisplayAlertAsync(
            "Confirmar exclusão",
            $"Deseja realmente excluir o produto {product.Name}?",
            "Sim",
            "Não");

        if (!confirm)
            return;

        try
        {
            if (!_productService.DeleteProduct(product.Id))
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível excluir o produto.",
                    "OK");
                return;
            }

            if (_editingProductId == product.Id)
                ResetForm();

            LoadProducts();

            await DisplayAlertAsync(
                "Sucesso",
                "Produto excluído com sucesso!",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void OnCancelEditClicked(object? sender, EventArgs e)
    {
        ResetForm();
    }

    private void OnRefreshClicked(object? sender, EventArgs e)
    {
        LoadProducts();
    }

    private void ResetForm()
    {
        _editingProductId = null;

        NameEntry.Text = string.Empty;
        DescriptionEntry.Text = string.Empty;
        CategoryEntry.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        StockEntry.Text = string.Empty;
        TagsEntry.Text = string.Empty;

        SaveProductButton.Text = "Cadastrar produto";
        CancelEditButton.IsVisible = false;
    }

    private void LoadProducts()
    {
        var products = _productService.GetProducts();

        ProductsList.Children.Clear();

        EmptyMessage.IsVisible = products.Count == 0;

        foreach (var product in products)
        {
            var editButton = new Button
            {
                Text = "Editar",
                BackgroundColor = Color.FromArgb("#2463EB"),
                TextColor = Colors.White,
                CornerRadius = 8,
                BindingContext = product
            };

            editButton.Clicked += OnEditProductClicked;

            var deleteButton = new Button
            {
                Text = "Excluir",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                CornerRadius = 8,
                BindingContext = product
            };

            deleteButton.Clicked += OnDeleteProductClicked;

            var cardContent = new VerticalStackLayout
            {
                Spacing = 7,
                Children =
                {
                    new Label
                    {
                        Text = product.Name,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#183153")
                    },
                    new Label
                    {
                        Text = $"Categoria: {product.Category}",
                        TextColor = Color.FromArgb("#5D6D7E")
                    },
                    new Label
                    {
                        Text = product.Description,
                        TextColor = Color.FromArgb("#5D6D7E")
                    },
                    new Label
                    {
                        Text = $"Preço: {product.Price:C}",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#16865B")
                    },
                    new Label
                    {
                        Text = $"Estoque: {product.Stock} unidades",
                        TextColor = Color.FromArgb("#5D6D7E")
                    },
                    new Label
                    {
                        Text = $"Tags: {string.Join(", ", product.Tags)}",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#85929E")
                    },
                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            editButton,
                            deleteButton
                        }
                    }
                }
            };

            var card = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#DCE3EB"),
                StrokeThickness = 1,
                Padding = 14,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 10
                },
                Content = cardContent
            };

            ProductsList.Children.Add(card);
        }
    }
}