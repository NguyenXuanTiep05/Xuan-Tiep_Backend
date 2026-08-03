
using System.Security.Claims;
using XuanTiepApi.Src.Services;
using XuanTiepApi.Src.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySqlConnector;
using XuanTiepApi.Src.Models.Finances;

namespace XuanTiepApi.Src.Controllers;


[Authorize]
[ApiController]
[Route("finance")]
public class FinanceController : ControllerBase
{
	private readonly ConnectionsStrings _connections;
	private string CurrentUserId =>
			User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1";


	public FinanceController(IOptions<ConnectionsStrings> connections)
	{
		_connections = connections.Value;
	}

	[HttpGet("summary")]
	public async Task<IActionResult> GetFinanceSummaryAsync()
	{
		try
		{
			var results = await Finances.FinanceSummaryAsync(_connections.Default!, CurrentUserId);
			if (results == null)
			{
				return NotFound(new { message = "There seems to be no records for you." });
			}
			return Ok(results);
		}
		catch (MySqlException ex)
		{
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}

	[HttpGet("income")]
	public async Task<IActionResult> GetFinanceIncomeAsync()
	{
		try
		{
			var resutls = await Finances.FinanceIncomeAsync(_connections.Default!, CurrentUserId);
			if (resutls == null)
			{
				return NotFound(new { message = "There seems to be no records for you." });
			}
			return Ok(resutls);

		}
		catch (MySqlException ex)
		{
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}

	[HttpGet("expenses")]
	public async Task<IActionResult> GetFinanceExpensesAsync()
	{
		try
		{
			var resutls = await Finances.FinanceExpensesAsync(_connections.Default!, CurrentUserId);
			if (resutls == null)
			{
				return NotFound(new { message = "There seems to be no records for you." });
			}
			return Ok(resutls);

		}
		catch (MySqlException ex)
		{
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}
[HttpGet("overview")]
public async Task<IActionResult> GetFinanceOverviewAsync()
{
    try
    {
        var results = await Finances.FinanceOverviewAsync(_connections.Default!, CurrentUserId);
        if (results == null)
        {
            return Ok(new FinanceOverviewResponseDto
            {
                Income = new List<FinanceRecordResponseDto>(),
                Expenses = new List<FinanceRecordResponseDto>(),
                Summary = new FinanceSummaryResponseDto { TotalIncome = 0, TotalExpenses = 0 }
            });
        }
        return Ok(results);
    }
    catch (MySqlException ex)
    {
        return StatusCode(500, new { message = "Database error.", detail = ex.Message });
    }
}


	[HttpPost("create_income")]
	public async Task<IActionResult> CreateFinanceIncome([FromBody] CreateFinanceRecordDto request)
	{
		try
		{
			long newId = await Finances.CreateFinanceIncome(_connections.Default!, CurrentUserId, request);
			return Ok(new { message = newId });
		}
		catch (MySqlException ex)
		{
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}

	[HttpPost("create_expense")]
	public async Task<IActionResult> CreateFinanceExpense([FromBody] CreateFinanceRecordDto request)
	{
		try
		{
			long newId = await Finances.CreateFinanceExpense(_connections.Default!, CurrentUserId, request);
			return Ok(new { message = newId });
		}
		catch (MySqlException ex)
		{
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}


	[HttpPost("del_finance_rec")]
	public async  Task<IActionResult> DeleteFinanceRecord([FromBody] DeleteFinanceRecordDto request)
	{
		try
		{
			await Finances.DeleteFinanceRecord(_connections.Default!, CurrentUserId, request);
			return Ok(new { message = "Record has been deleted." });
		}
		catch (MySqlException ex)
		{
			Console.WriteLine(ex.Message);
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}
}







