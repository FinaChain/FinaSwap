using System.Numerics;
using FinaSwap.Api.Models;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace FinaSwap.Api.Services
{
    public class EtherSendService : IEtherSendService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<SwapConfiguration> _swapConfiguration;

        public EtherSendService(IHttpClientFactory httpClientFactory, IOptions<SwapConfiguration> swapConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            _swapConfiguration = swapConfiguration;
        }

        public Task Send(string address)
        {
            if (_swapConfiguration.Value.EtherAmountWei == 0)
                return Task.CompletedTask;

            var httpClient = _httpClientFactory.CreateClient("RPC");
            var account = new Account(_swapConfiguration.Value.PrivateKey, new BigInteger(_swapConfiguration.Value.ChainId))
            {
                TransactionManager =
                {
                    UseLegacyAsDefault = true
                }
            };
            var rpcClient = new RpcClient(httpClient.BaseAddress, httpClient);
            var web3 = new Web3(account, rpcClient);

            return web3.Eth.TransactionManager.TransactionReceiptService.SendRequestAndWaitForReceiptAsync(
                new TransactionInput
                {
                    From = account.Address,
                    To = address,
                    Value = new HexBigInteger(_swapConfiguration.Value.EtherAmountWei)
                });
        }
    }
}
