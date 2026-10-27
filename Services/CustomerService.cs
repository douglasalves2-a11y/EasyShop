using EasyShop.Data.Sql;
using EasyShop.Models;

namespace EasyShop.Services;

public class CustomerService
{
    private readonly CustomerRepository _repository;

    public CustomerService()
    {
        _repository = new CustomerRepository();
    }

    public async Task InitializeAsync()
    {
        await _repository.InitializeAsync();
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("O ID do cliente deve ser válido.");

        return await _repository.GetByIdAsync(id);
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        ValidateCustomer(customer);

        var existingCustomers = await _repository.GetAllAsync();

        bool emailExists = existingCustomers.Any(
            existing => string.Equals(
                existing.Email.Trim(),
                customer.Email.Trim(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (emailExists)
            throw new InvalidOperationException(
                "Já existe um cliente cadastrado com esse e-mail."
            );

        customer.Name = customer.Name.Trim();
        customer.Email = customer.Email.Trim();
        customer.Phone = customer.Phone.Trim();

        await _repository.AddAsync(customer);
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        ValidateCustomer(customer);

        if (customer.Id <= 0)
            throw new ArgumentException("O ID do cliente deve ser válido.");

        var existingCustomers = await _repository.GetAllAsync();

        bool emailExists = existingCustomers.Any(
            existing => existing.Id != customer.Id &&
                        string.Equals(
                            existing.Email.Trim(),
                            customer.Email.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        )
        );

        if (emailExists)
            throw new InvalidOperationException(
                "Esse e-mail já está sendo utilizado por outro cliente."
            );

        customer.Name = customer.Name.Trim();
        customer.Email = customer.Email.Trim();
        customer.Phone = customer.Phone.Trim();

        await _repository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("O ID do cliente deve ser válido.");

        var customer = await _repository.GetByIdAsync(id);

        if (customer is null)
            throw new InvalidOperationException("Cliente não encontrado.");

        await _repository.DeleteAsync(customer);
    }

    private static void ValidateCustomer(Customer customer)
    {
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));

        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Informe o nome do cliente.");

        if (string.IsNullOrWhiteSpace(customer.Email))
            throw new ArgumentException("Informe o e-mail do cliente.");

        if (!customer.Email.Contains('@') ||
            !customer.Email.Contains('.'))
        {
            throw new ArgumentException("Informe um e-mail válido.");
        }
    }
}