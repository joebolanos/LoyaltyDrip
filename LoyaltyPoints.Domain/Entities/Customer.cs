namespace LoyaltyPoints.Domain.Entities;

/// <summary>
/// Represents a registered customer in the platform.
/// This is the root entity from which all loyalty balances hang.
/// </summary>
public class Customer
{
    /// <summary>Primary key — varchar(10) assigned externally (e.g. CUST000001).</summary>
    public string CustomerId { get; private set; }

    /// <summary>Full display name of the customer.</summary>
    public string FullName { get; private set; }

    /// <summary>Unique email address.</summary>
    public string Email { get; private set; }

    /// <summary>Whether the customer account is active.</summary>
    public bool IsActive { get; private set; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>Required by Dapper for mapping query results.</summary>
    private Customer() { }

    public Customer(string customerId, string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        if (customerId.Length > 10)
            throw new ArgumentException("CustomerId cannot exceed 10 characters.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("FullName is required.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        CustomerId = customerId;
        FullName   = fullName;
        Email      = email;
        IsActive   = true;
        CreatedAt  = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Domain behaviour
    // -------------------------------------------------------------------------

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
