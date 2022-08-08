namespace FinaSwap.Api.Models
{
    public class ClaimResult : IClaimResult
    {
        public string? TransactionHash { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
