using System.Text.RegularExpressions;
using FinaSwap.Api.Models;
using FinaSwap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinaSwap.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;
        private readonly IEtherSendService _etherSendService;
        private readonly IBase58ValidationService _base58ValidationService;
        private readonly IOptions<SwapConfiguration> _swapConfiguration;


        public ClaimController(IClaimService claimService, IEtherSendService etherSendService, IBase58ValidationService base58ValidationService, IOptions<SwapConfiguration> swapConfiguration)
        {
            _claimService = claimService;
            _etherSendService = etherSendService;
            _base58ValidationService = base58ValidationService;
            _swapConfiguration = swapConfiguration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string target,
            [FromQuery] string source,
            [FromQuery] string hash, 
            [FromQuery] string signature)
        {
            if (DateTime.UtcNow < _swapConfiguration.Value.StartDate)
                return BadRequest($"Swap has not been started and will start at {_swapConfiguration.Value.StartDate}");
            if (string.IsNullOrWhiteSpace(source))
                return BadRequest("Source address is required");
            if (!source.StartsWith(_swapConfiguration.Value.Prefix))
                return BadRequest("Invalid source address prefix");
            if (source.Length != 34)
                return BadRequest("Invalid source address length");
            if (!_base58ValidationService.IsValid(source))
                return BadRequest("Invalid source address encoding");
            if (string.IsNullOrWhiteSpace(target))
                return BadRequest("Target address is required");
            if (string.IsNullOrWhiteSpace(hash))
                return BadRequest("Hash is required");
            if (string.IsNullOrWhiteSpace(signature))
                return BadRequest("Signature is required");
            if (!Nethereum.Util.AddressUtil.Current.IsValidAddressLength(target))
                return BadRequest("Invalid target address length");
            if (!Nethereum.Util.AddressUtil.Current.IsValidEthereumAddressHexFormat(target))
                return BadRequest("Invalid target address encoding");
            if (hash.Length != 66)
                return BadRequest("Invalid hash length");
            if (signature.Length != 132)
                return BadRequest("Invalid signature length");
            if (!hash.StartsWith("0x"))
                return BadRequest("Invalid hash hex prefix (should start with 0x)");
            if (!signature.StartsWith("0x"))
                return BadRequest("Invalid signature hex prefix (should start with 0x)");
            if (!Regex.IsMatch(hash, "^(0x)[0-9a-fA-F]{64}"))
                return BadRequest("Invalid hash format");
            if (!Regex.IsMatch(signature, "^(0x)[0-9a-fA-F]{130}"))
                return BadRequest("Invalid signature format");

            var claimResult = await _claimService.Claim(target, source, hash, signature);

            if (!claimResult.IsSuccess && claimResult.Message.StartsWith("Smart contract error:"))
                return BadRequest(claimResult.Message);

            if (!claimResult.IsSuccess)
                return BadRequest("An unknown error has occurred");

            await _etherSendService.Send(target);

            return Ok(claimResult.TransactionHash);
        }
    }
}
