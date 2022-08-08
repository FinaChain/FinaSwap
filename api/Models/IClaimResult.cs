namespace FinaSwap.Api.Models;

public interface IClaimResult
{
    string? TransactionHash { get; set; }
    bool IsSuccess { get; set; }
    string Message { get; set; }
}