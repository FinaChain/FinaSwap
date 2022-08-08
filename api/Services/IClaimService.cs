using FinaSwap.Api.Models;

namespace FinaSwap.Api.Services;

public interface IClaimService
{
    Task<IClaimResult> Claim(string target, string source, string hash, string signature);
}