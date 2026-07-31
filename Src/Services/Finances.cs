


using MySqlConnector;
using Auth.Src.Models.Finances;

namespace Auth.Src.Services;



public static class Finances
{
	public async static Task<FinanceSummaryResponseDto?> FinanceSummaryAsync(string connectionStr, string userId)
	{
		using var connection = new MySqlConnection(connectionStr);
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
		command.Parameters.AddWithValue("@id", userId);
		var results = new List<FinanceSummaryResponseDto>();

		using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			results.Add(new FinanceSummaryResponseDto
			{
				UserId = reader.GetInt32("user_id"),
				Currency = reader.GetString("currency"),
				TotalIncome = reader.GetDouble("total_income"),
				TotalExpenses = reader.GetDouble("total_expenses"),
				Net = reader.GetDouble("net")
			});
		}

		if (results.Count < 1) return null;

		return results[0];
	}

	public async static Task<List<FinanceRecordResponseDto>?> FinanceIncomeAsync(string connectionStr, string userId)
	{
		using var connection = new MySqlConnection(connectionStr);
		await connection.OpenAsync();

		using var command = new MySqlCommand("""
				SELECT id, value, currency, description, date_ FROM finance_income
				WHERE date_ >= DATE_FORMAT(CURDATE(), '%Y-%m-01')  AND date_ < DATE_FORMAT(CURDATE() + INTERVAL 1 MONTH, '%Y-%m-01')
				AND user_id = @userId
				ORDER BY date_ DESC;
		""", connection);

		command.Parameters.AddWithValue("@userId", userId);

		using var reader = await command.ExecuteReaderAsync();
		var results = new List<FinanceRecordResponseDto>();

		while (await reader.ReadAsync())
		{
			results.Add(new FinanceRecordResponseDto
			{
				RecordId = reader.GetInt32("id"),
				Value = reader.GetDecimal("value"),
				Currency = reader.GetString("currency"),
				Description = reader.GetString("description"),
				Date = reader.GetDateTime("date_"),
				Type = "income"
			});
		}

		if (results.Count < 1) return null;

		return results;

	}
	public async static Task<List<FinanceRecordResponseDto>?> FinanceExpensesAsync(string connectionStr, string userId)
	{
		using var connection = new MySqlConnection(connectionStr);
		await connection.OpenAsync();

		using var command = new MySqlCommand("""
				SELECT id, value, currency, description, date_ FROM finance_expenses
				WHERE date_ >= DATE_FORMAT(CURDATE(), '%Y-%m-01')  AND date_ < DATE_FORMAT(CURDATE() + INTERVAL 1 MONTH, '%Y-%m-01')
				AND user_id = @userId
				ORDER BY date_ DESC;
		""", connection);

		command.Parameters.AddWithValue("@userId", userId);

		using var reader = await command.ExecuteReaderAsync();
		var results = new List<FinanceRecordResponseDto>();

		while (await reader.ReadAsync())
		{
			results.Add(new FinanceRecordResponseDto
			{
				RecordId = reader.GetInt32("id"),
				Value = reader.GetDecimal("value"),
				Currency = reader.GetString("currency"),
				Description = reader.GetString("description"),
				Date = reader.GetDateTime("date_"),
				Type = "expense"
			});
		}

		if (results.Count < 1) return null;

		return results;

	}


	public static async Task<FinanceOverviewResponseDto?> FinanceOverviewAsync(string connection, string userId)
	{
		var income = await FinanceIncomeAsync(connection, userId);
		var expenses = await FinanceExpensesAsync(connection, userId);
		var summary = await FinanceSummaryAsync(connection, userId);
		if (summary == null) return null;

		var results = new FinanceOverviewResponseDto
		{
			Income = income,
			Expenses = expenses,
			Summary = summary
		};

		return results;
	}



	public static async Task<long> CreateFinanceIncome(string connectionStr, string userId, CreateFinanceRecordDto incomeRequest)
	{
		using var connection = new MySqlConnection(connectionStr);
		await connection.OpenAsync();
		using var command = new MySqlCommand("""
					INSERT INTO finance_income (value, currency, description, date_, user_id)
					VALUES (@value, "CZK", @description, NOW(), @user_id);
					SELECT LAST_INSERT_ID();
					""", connection);
		command.Parameters.AddWithValue("@value", incomeRequest.Value);
		command.Parameters.AddWithValue("@description", incomeRequest.Description);
		command.Parameters.AddWithValue("@user_id", userId);


		  return Convert.ToInt64(await command.ExecuteScalarAsync());
	}
	public static async Task<long> CreateFinanceExpense(string connectionStr, string userId, CreateFinanceRecordDto incomeRequest)
    {
        using var connection = new MySqlConnection(connectionStr);
        await connection.OpenAsync();
        using var command = new MySqlCommand("""
                    INSERT INTO finance_expenses (value, currency, description, date_, user_id)
                    VALUES (@value, "CZK", @description, NOW(), @user_id); SELECT LAST_INSERT_ID();
                    """, connection);
        command.Parameters.AddWithValue("@value", incomeRequest.Value);
        command.Parameters.AddWithValue("@description", incomeRequest.Description);
        command.Parameters.AddWithValue("@user_id", userId);
          return Convert.ToInt64(await command.ExecuteScalarAsync());
    }


	public static async Task DeleteFinanceRecord(string connectionStr, string userId, DeleteFinanceRecordDto deleteRequest)
	{
		using var connection = new MySqlConnection(connectionStr);
		await connection.OpenAsync();
	    string table = deleteRequest.Type switch
	    {
	        "income" => "finance_income",
	        "expense" => "finance_expenses",
	        _ => throw new ArgumentException("This is not a valid record type.")
	    };
		using var command = new MySqlCommand($"""
					Delete FROM {table} where id = @record AND user_id = @user_id;
					""", connection);
		command.Parameters.AddWithValue("@user_id", userId);
		command.Parameters.AddWithValue("@record", deleteRequest.Id);


		await command.ExecuteNonQueryAsync();
	}



}



