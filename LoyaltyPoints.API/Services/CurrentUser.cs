using LoyaltyPoints.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LoyaltyPoints.API.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? CustomerId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("customerId");
}
