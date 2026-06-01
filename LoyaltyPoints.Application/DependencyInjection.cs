using System.Reflection;
using FluentValidation;
using LoyaltyPoints.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyPoints.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehavior<>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });
        return services;
    }
}
