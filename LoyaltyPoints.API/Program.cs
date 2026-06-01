using Asp.Versioning;
using LoyaltyPoints.API.Endpoints.Drip;
using LoyaltyPoints.API.Endpoints.Internal;
using LoyaltyPoints.API.Services;
using LoyaltyPoints.Application;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUser, CurrentUser>();

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1);
    opt.ReportApiVersions = true;
    opt.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "LoyaltyDrip API";
        options.Theme = ScalarTheme.DeepSpace;
    });
}

app.UseHttpsRedirection();

var v1 = app.NewVersionedApi();

var drip = v1.MapGroup("/v{version:apiVersion}/drip");
drip.MapClaimEndpoints();
drip.MapGetBalancesEndpoints();

var internalGroup = v1.MapGroup("/v{version:apiVersion}/internal/drip");
internalGroup.MapNightlyJobEndpoints();

app.Run();
