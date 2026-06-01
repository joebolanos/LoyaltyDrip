using LoyaltyPoints.API.Endpoints.Drip;
using LoyaltyPoints.API.Endpoints.Internal;
using LoyaltyPoints.Application;
using LoyaltyPoints.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Default")!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

ClaimEndpoint.Map(app);
GetBalancesEndpoint.Map(app);
NightlyJobEndpoint.Map(app);

app.Run();
