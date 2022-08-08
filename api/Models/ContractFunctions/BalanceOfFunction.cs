using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace FinaSwap.Api.Models.ContractFunctions
{
    [Function("balanceOf", "uint256")]
    public class BalanceOfFunction : FunctionMessage
    {
        [Parameter("bytes", "source")] 
        public byte[] Source { get; set; } = Array.Empty<byte>();
    }
}
