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

var swapConfiguration = app.Services.GetRequiredService<IOptions<SwapConfiguration>>();
Console.WriteLine("---- Configuration ----");
Console.WriteLine("Contract address: {0}", swapConfiguration.Value.ContractAddress);
Console.WriteLine("RPC endpoint: {0}", swapConfiguration.Value.RpcEndpoint);
Console.WriteLine("ChainId: {0}", swapConfiguration.Value.ChainId);
Console.WriteLine("Decimal places: {0}", swapConfiguration.Value.DecimalPlaces);
Console.WriteLine("Prefix: {0}", swapConfiguration.Value.Prefix);
Console.WriteLine("Ether amount: {0}", swapConfiguration.Value.EtherAmountWei);
Console.WriteLine("Start date: {0}", swapConfiguration.Value.StartDate);
Console.WriteLine("-----------------------");


app.Run();
