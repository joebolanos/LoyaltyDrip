using System.Data;
using Dapper;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Repositories;
using LoyaltyPoints.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyPoints.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILPBalanceRepository, LPBalanceRepository>();
        services.AddScoped<IDripPoolRepository, DripPoolRepository>();
        services.AddScoped<IUnclaimedPoolRepository, UnclaimedPoolRepository>();
        services.AddScoped<IDailyClaimSnapshotRepository, DailyClaimSnapshotRepository>();

        return services;
    }

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value) =>
            DateOnly.FromDateTime((DateTime)value);

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value  = value.ToDateTime(TimeOnly.MinValue);
        }
    }
}
