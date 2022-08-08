namespace FinaSwap.Api.Models
{
    public abstract class SwapBase
    {
        public string? Signature { get; set; }
        public string? Hash { get; set; }
    }
}
