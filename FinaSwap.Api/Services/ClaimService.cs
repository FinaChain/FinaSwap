using FinaSwap.Api.Extensions;
using FinaSwap.Api.Models;
using FinaSwap.Api.Models.ContractFunctions;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace FinaSwap.Api.Services;

public class ClaimService : IClaimService
{
    private readonly IOptions<SwapConfiguration> _swapConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ClaimService(IOptions<SwapConfiguration> swapConfiguration, IHttpClientFactory httpClientFactory)
    {
        _swapConfiguration = swapConfiguration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IClaimResult> Claim(string target, string source, string hash, string signature)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("RPC");
            var account = new Account(_swapConfiguration.Value.PrivateKey, _swapConfiguration.Value.ChainId)
            {
                TransactionManager =
                {
                    UseLegacyAsDefault = true
                }
            };

            var rpcClient = new RpcClient(httpClient.BaseAddress, httpClient);
            var web3 = new Web3(account, rpcClient);

            var claimFunction = new ClaimFunction
            {
                Target = target,
                Source = source.Base58Decode(),
                Hash = hash.HexToByteArray(),
                Signature = signature.HexToByteArray()
            };

            var claimHandler = web3.Eth.GetContractTransactionHandler<ClaimFunction>();
        
            var receipt = await claimHandler.SendRequestAndWaitForReceiptAsync(_swapConfiguration.Value.ContractAddress, claimFunction);

            return new ClaimResult
            {
                TransactionHash = receipt.TransactionHash,
                IsSuccess = true
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ClaimResult
            {
                IsSuccess = false,
                Message = e.Message
            };
        }
    }
}