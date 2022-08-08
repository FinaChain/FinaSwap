namespace FinaSwap.Api.Services;

public interface IBalanceOfService
{
    Task<decimal> GetBalanceOf(string address);
}