using System.Data;
using Dapper;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Infrastructure.Persistance;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyPoints.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
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
