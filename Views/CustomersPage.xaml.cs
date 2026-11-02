using EasyShop.Models;
using EasyShop.Services;

namespace EasyShop.Views;

public partial class CustomersPage : ContentPage
{
    private readonly CustomerService _customerService;

    private int? _editingCustomerId;

    public CustomersPage()
    {
        InitializeComponent();

        _customerService = new CustomerService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _customerService.InitializeAsync();
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível carregar os clientes: {ex.Message}",
                "OK");
        }
    }

    private async void OnAddCustomerClicked(object? sender, EventArgs e)
    {
        try
        {
            var customer = new Customer
            {
                Name = NameEntry.Text ?? string.Empty,
                Email = EmailEntry.Text ?? string.Empty,
                Phone = PhoneEntry.Text ?? string.Empty
            };

            if (_editingCustomerId.HasValue)
            {
                customer.Id = _editingCustomerId.Value;

                await _customerService.UpdateCustomerAsync(customer);

                ResetForm();

                await LoadCustomersAsync();

                await DisplayAlertAsync(
                    "Sucesso",
                    "Cliente atualizado com sucesso!",
                    "OK");
            }
            else
            {
                await _customerService.AddCustomerAsync(customer);

                ResetForm();

                await LoadCustomersAsync();

                await DisplayAlertAsync(
                    "Sucesso",
                    "Cliente cadastrado com sucesso!",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                ex.Message,
                "OK");
        }
    }

    private async void OnEditCustomerClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button ||
                button.BindingContext is not Customer selectedCustomer)
            {
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(
                selectedCustomer.Id);

            if (customer is null)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Cliente não encontrado.",
                    "OK");

                await LoadCustomersAsync();
                return;
            }

            _editingCustomerId = customer.Id;

            NameEntry.Text = customer.Name;
            EmailEntry.Text = customer.Email;
            PhoneEntry.Text = customer.Phone;

            SaveCustomerButton.Text = "Salvar alterações";
            CancelEditButton.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                ex.Message,
                "OK");
        }
    }

    private async void OnDeleteCustomerClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button ||
                button.BindingContext is not Customer customer)
            {
                return;
            }

            bool confirm = await DisplayAlertAsync(
                "Confirmar exclusão",
                $"Deseja realmente excluir o cliente {customer.Name}?",
                "Sim",
                "Não");

            if (!confirm)
                return;

            await _customerService.DeleteCustomerAsync(customer.Id);

            if (_editingCustomerId == customer.Id)
                ResetForm();

            await LoadCustomersAsync();

            await DisplayAlertAsync(
                "Sucesso",
                "Cliente excluído com sucesso!",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                ex.Message,
                "OK");
        }
    }

    private void OnCancelEditClicked(object? sender, EventArgs e)
    {
        ResetForm();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await LoadCustomersAsync();
    }

    private void ResetForm()
    {
        _editingCustomerId = null;

        NameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;

        SaveCustomerButton.Text = "Cadastrar cliente";
        CancelEditButton.IsVisible = false;
    }

    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetCustomersAsync();

        CustomersList.Children.Clear();

        EmptyMessage.IsVisible = customers.Count == 0;

        foreach (var customer in customers)
        {
            var nameLabel = new Label
            {
                Text = customer.Name,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#183153")
            };

            var emailLabel = new Label
            {
                Text = customer.Email,
                FontSize = 14,
                TextColor = Color.FromArgb("#5D6D7E")
            };

            var phoneLabel = new Label
            {
                Text = string.IsNullOrWhiteSpace(customer.Phone)
                    ? "Telefone não informado"
                    : customer.Phone,
                FontSize = 14,
                TextColor = Color.FromArgb("#5D6D7E")
            };

            var editButton = new Button
            {
                Text = "Editar",
                BackgroundColor = Color.FromArgb("#2463EB"),
                TextColor = Colors.White,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.Fill,
                BindingContext = customer
            };

            editButton.Clicked += OnEditCustomerClicked;

            var deleteButton = new Button
            {
                Text = "Excluir",
                BackgroundColor = Color.FromArgb("#DC3545"),
                TextColor = Colors.White,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.Fill,
                BindingContext = customer
            };

            deleteButton.Clicked += OnDeleteCustomerClicked;

            var buttonsLayout = new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    editButton,
                    deleteButton
                }
            };

            var customerLayout = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    nameLabel,
                    emailLabel,
                    phoneLabel,
                    buttonsLayout
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
                Content = customerLayout
            };

            CustomersList.Children.Add(card);
        }
    }
}