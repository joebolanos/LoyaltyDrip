using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Domain.Repositories;

/// <summary>Read-only access to the Customer aggregate root.</summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string customerId);
}
