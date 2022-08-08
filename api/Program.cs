using FinaSwap.Api.Models;
using FinaSwap.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<SwapConfiguration>(builder.Configuration.GetSection("SwapConfiguration"));
builder.Services.AddScoped<IBalanceOfService, BalanceOfService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IEtherSendService, EtherSendService>();
builder.Services.AddScoped<IBase58ValidationService, Base58ValidationService>();

builder.Services.AddHttpClient("RPC", (serviceProvider, httpClient) =>
{
    var configuration = serviceProvider.GetRequiredService<IOptions<SwapConfiguration>>();
    httpClient.BaseAddress = configuration.Value.RpcEndpoint;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
