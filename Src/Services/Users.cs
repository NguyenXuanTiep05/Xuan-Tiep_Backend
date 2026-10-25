


using System.Text;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

namespace XuanTiepApi.Src.Services;



public static class Users
{
	public static void GetUserData(string connectionStr,string token, out Guid? id, out string? role, out string? username)
	{
		using var connection = new MySqlConnection(connectionStr);
		connection.Open();
		using var command = new MySqlCommand("SELECT b.id, b.role, b.username FROM db.refresh_tokens AS a INNER JOIN db.users AS b ON a.user_id = b.id WHERE a.token_hash = @token", connection);
		command.Parameters.AddWithValue("@token", HashToken(token));
		using MySqlDataReader reader = command.ExecuteReader();
		if (!reader.Read())
		{
		    throw new InvalidOperationException("User doesnt exists");
		}
		id = reader.GetGuid("id");
		role = reader.GetString("role");
		username = reader.GetString("username");

	}
	public static string HashToken(string token) =>
	    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}



