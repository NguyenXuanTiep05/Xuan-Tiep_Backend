using System.Reflection.Metadata;
using MySqlConnector;
using BCrypt.Net;

using Auth.src.Models;

public static class Password
{
	private static string _connectionString = "";
	public static void SetConn(string conn) { _connectionString = conn; }
	public static bool Compare(LoginRequest lgr)
	{
		using var connection = new MySqlConnection(_connectionString);
		connection.Open();
		var command = new MySqlCommand("Select password from users where username = @username", connection);
		command.Parameters.Add("@username", MySqlDbType.VarChar).Value = lgr.Username.ToLower();
		using MySqlDataReader reader = command.ExecuteReader();
		if (reader.Read())
		{
			string storedHash = reader.GetString("password");

			bool isValid = BCrypt.Net.BCrypt.Verify(lgr.Password, storedHash);
			return isValid;

		}

		return false;
	}
}