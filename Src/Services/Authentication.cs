


using System.Text;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

namespace XuanTiepApi.Src.Services;



public static class Authentication
{
	public static string GenerateJwtToken(string username, int id, string role, Jwt jwt)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.Name, username),
			new Claim(ClaimTypes.Role, role),
			new Claim(ClaimTypes.NameIdentifier, id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var token = new JwtSecurityToken(
			issuer: jwt.Issuer,
			audience: jwt.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(15),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
	public static void SaveRefreshToken(string connectionStr,string token, string username)
	{
		token = HashToken(token);
		using var connection = new MySqlConnection(connectionStr);
		connection.Open();
		using var command = new MySqlCommand("""
		    INSERT INTO db.refresh_tokens (token_hash, user_id, valid_to, created_at)
		    SELECT @tokenHash, id, UTC_TIMESTAMP() + INTERVAL 30 DAY, UTC_TIMESTAMP()
		    FROM db.users
		    WHERE username = @username
		    LIMIT 1;
		    """, connection);

		command.Parameters.AddWithValue("@tokenHash", token);
		command.Parameters.AddWithValue("@username", username);
		command.ExecuteNonQuery();
	}
	public static void DeleteRefreshToken(string connectionStr,string token)
	{
		token = HashToken(token);
		using var connection = new MySqlConnection(connectionStr);
		connection.Open();
		using var command = new MySqlCommand("""
		    UPDATE db.refresh_tokens
		    SET revoked_at = UTC_TIMESTAMP() WHERE token_hash = @token_hash ;
		    """, connection);

		command.Parameters.AddWithValue("@token_hash", token);
		command.ExecuteNonQuery();
	}

	public static void UpdateRefreshToken(string connectionStr,string oldToken,string newToken)
	{
		using var connection = new MySqlConnection(connectionStr);
		connection.Open();
		using var command = new MySqlCommand("""
		    UPDATE db.refresh_tokens
		    SET token_hash = @tokenHash WHERE token_hash = @oldTokenHash AND revoked_at IS NULL AND valid_to >= UTC_TIMESTAMP();
		    """, connection);

		command.Parameters.AddWithValue("@tokenHash", HashToken(newToken));
		command.Parameters.AddWithValue("@oldTokenHash",HashToken(oldToken));
		if (command.ExecuteNonQuery() != 1)
		{
			throw new InvalidOperationException("Refresh token was doesnt exist");
		}
	}
	public static string HashToken(string token) =>
	    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}



