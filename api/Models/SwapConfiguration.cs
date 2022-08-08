namespace FinaSwap.Api.Models
{
    public class SwapConfiguration
    {
        public string? ContractAddress { get; set; }
        public Uri? RpcEndpoint { get; set; }
        public string? PrivateKey { get; set; }
        public long ChainId { get; set; }
        public int DecimalPlaces { get; set; }
        public char Prefix { get; set; }
        public long EtherAmountWei { get; set; }
        public DateTime StartDate { get; set; }
    }
}
