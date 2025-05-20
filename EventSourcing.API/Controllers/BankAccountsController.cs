using Microsoft.AspNetCore.Mvc;
using EventSourcing.Application.Services;

namespace EventSourcing.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController(IBankAccountService _bankAccountService) : ControllerBase
    {
        [HttpPost("open")]
        public async Task<IActionResult> OpenAccount([FromQuery] string accountHolder, [FromQuery] decimal initialDeposit)
        {
            try
            {
                var id = await _bankAccountService.OpenAccountAsync(accountHolder, initialDeposit);
                return Ok(new { id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{accountId}/balance")]
        public async Task<IActionResult> GetBalance(string accountId)
        {
            try
            {
                var (balance, isActive) = await _bankAccountService.GetBalanceAsync(accountId);
                return Ok(new { balance, isActive });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{accountId}/deposit")]
        public async Task<IActionResult> Deposit(string accountId, [FromQuery] decimal amount)
        {
            try
            {
                await _bankAccountService.DepositAsync(accountId, amount);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{accountId}/withdraw")]
        public async Task<IActionResult> Withdraw(string accountId, [FromQuery] decimal amount)
        {
            try
            {
                await _bankAccountService.WithdrawAsync(accountId, amount);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
