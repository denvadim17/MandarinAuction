using MandarinAuction.Api.DTOs.Wallet;
using MandarinAuction.Api.Extensions;
using MandarinAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly WalletService _walletService;
    private readonly CashbackService _cashbackService;

    public WalletController(WalletService walletService, CashbackService cashbackService)
    {
        _walletService = walletService;
        _cashbackService = cashbackService;
    }

    [HttpGet]
    public async Task<ActionResult<WalletDto>> GetWallet()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var (balance, purchased) = await _walletService.GetWalletSummaryAsync(userId);
        var cashbackPercent = await _cashbackService.CalculateCashbackPercentAsync(userId);
        return Ok(new WalletDto(balance, purchased, cashbackPercent));
    }

    [HttpPost("deposit")]
    public async Task<ActionResult> Deposit(DepositRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        await _walletService.DepositAsync(userId, request.Amount);
        return Ok(new { message = $"Баланс пополнен на {request.Amount}" });
    }

    [HttpPost("payment-link")]
    public ActionResult GeneratePaymentLink([FromBody] DepositRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var link = _walletService.GeneratePaymentLink(userId, request.Amount);
        return Ok(new { paymentLink = link, qrData = link });
    }

    [HttpGet("confirm-deposit")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmDeposit(
        [FromQuery] string userId,
        [FromQuery] decimal amount,
        [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || amount <= 0 || string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Некорректные параметры" });

        await _walletService.DepositAsync(userId, amount);
        return Ok(new { message = $"Баланс пополнен на {amount}" });
    }
}
