using FinaSwap.Api.Models;
using FinaSwap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinaSwap.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BalanceOfController : ControllerBase
{
    private readonly IBalanceOfService _swapBalanceService;
    private readonly IBase58ValidationService _base58ValidationService;
    private readonly IOptions<SwapConfiguration> _swapConfiguration;

    public BalanceOfController(IBalanceOfService swapBalanceService, IBase58ValidationService base58ValidationService, IOptions<SwapConfiguration> swapConfiguration)
    {
        _swapBalanceService = swapBalanceService;
        _base58ValidationService = base58ValidationService;
        _swapConfiguration = swapConfiguration;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return BadRequest("Address is required");
        if (!address.StartsWith(_swapConfiguration.Value.Prefix))
            return BadRequest("Invalid address prefix");
        if (address.Length != 34)
            return BadRequest("Invalid address length");
        if (!_base58ValidationService.IsValid(address))
            return BadRequest("Invalid address encoding");

        return Ok(await _swapBalanceService.GetBalanceOf(address));
    }
}