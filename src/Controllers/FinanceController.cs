
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Auth.src.Controllers;



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


	[Authorize]
	[HttpGet("overview")]
	public async Task<IActionResult> GetFinanceDataAsync()
	{
		using var connection = new MySqlConnection(_connections.Default);
		try
		{
			await connection.OpenAsync();

			using var command = new MySqlCommand("""
			    SELECT
			        i.user_id,
			        i.currency,
			        COALESCE(i.total_income, 0) AS total_income,
			        COALESCE(e.total_expenses, 0) AS total_expenses,
			        COALESCE(i.total_income, 0) - COALESCE(e.total_expenses, 0) AS net
			    FROM (
			        SELECT user_id, currency, SUM(value) AS total_income
			        FROM finance_income
			        WHERE date_ >= DATE_FORMAT(CURDATE(), '%Y-%m-01')
			          AND date_ < DATE_FORMAT(CURDATE() + INTERVAL 1 MONTH, '%Y-%m-01')
			        GROUP BY user_id, currency
			    ) i
			    LEFT JOIN (
			        SELECT user_id, currency, SUM(value) AS total_expenses
			        FROM finance_expenses 
			        WHERE date_ >= DATE_FORMAT(CURDATE(), '%Y-%m-01')
			          AND date_ < DATE_FORMAT(CURDATE() + INTERVAL 1 MONTH, '%Y-%m-01')
			        GROUP BY user_id, currency
			    ) e ON i.user_id = e.user_id AND i.currency = e.currency
			    WHERE i.user_id = @id
			    """, connection);
			command.Parameters.AddWithValue("@id", CurrentUserId);
			var results = new List<FinanceSummaryDto>();

			using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				results.Add(new FinanceSummaryDto
				{
					UserId = reader.GetInt32("user_id"),
					Currency = reader.GetString("currency"),
					TotalIncome = reader.GetDouble("total_income"),
					TotalExpenses = reader.GetDouble("total_expenses"),
					Net = reader.GetDouble("net")
				});
			}

			return Ok(results);
		}
		catch (MySqlException ex)
		{
			return StatusCode(500, new { message = "Database error.", detail = ex.Message });
		}
	}
}







