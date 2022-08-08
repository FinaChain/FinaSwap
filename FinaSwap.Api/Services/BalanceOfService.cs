using System.Numerics;
using FinaSwap.Api.Extensions;
using FinaSwap.Api.Models;
using FinaSwap.Api.Models.ContractFunctions;
using Microsoft.Extensions.Options;
using Nethereum.JsonRpc.Client;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace FinaSwap.Api.Services;

public class BalanceOfService : IBalanceOfService
{
    private readonly IOptions<SwapConfiguration> _swapConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;
    public BalanceOfService(IOptions<SwapConfiguration> swapConfiguration, IHttpClientFactory httpClientFactory)
    {
        _swapConfiguration = swapConfiguration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<decimal> GetBalanceOf(string address)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("RPC");
            var account = new Account(_swapConfiguration.Value.PrivateKey, new BigInteger(_swapConfiguration.Value.ChainId));
            var rpcClient = new RpcClient(httpClient.BaseAddress, httpClient);
            var web3 = new Web3(account, rpcClient);

            var balanceOfFunction = new BalanceOfFunction
            {
                Source = address.Base58Decode()
            };

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(_swapConfiguration.Value.ContractAddress, balanceOfFunction);

            return UnitConversion.Convert.FromWei(balance, _swapConfiguration.Value.DecimalPlaces);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0m;
        }
    }
}