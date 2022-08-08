using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace FinaSwap.Api.Models.ContractFunctions;

[Function("claim")]
public class ClaimFunction : FunctionMessage
{
    [Parameter("address", "target", 1)]
    public string? Target { get; set; }
    [Parameter("bytes", "source", 2)]
    public byte[] Source { get; set; } = Array.Empty<byte>();

    [Parameter("bytes32", "hash", 3)]
    public byte[] Hash { get; set; } = Array.Empty<byte>();

    [Parameter("bytes", "signature", 4)]
    public byte[] Signature { get; set; } = Array.Empty<byte>();
}