using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using BCrypt.Net;

using XuanTiepApi.Src.Services;
using XuanTiepApi.Src.Models;
using Microsoft.AspNetCore.RateLimiting;

namespace XuanTiepApi.Src.Controllers;

[Authorize]
[ApiController]
[Route("auth")]
public class AuthController  : ControllerBase
{

	private readonly Jwt _jwt;
	private readonly ConnectionsStrings _connStr;

	public AuthController(
		IOptions<Jwt> jwt,
		IOptions<ConnectionsStrings> connStr)
	{
		_jwt = jwt.Value;
		_connStr = connStr.Value;
		Password.SetConn(_connStr.Default ?? "");
	}

	[AllowAnonymous]
	[HttpPost("login")]
	[EnableRateLimiting("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		if (!Password.Compare(request, out int? id, out string? role)) { return NotFound(new { message = "Invalid Credentials" }); }

		var token = GenerateJwtToken(request.Username, id ?? -1, role!);
		Response.Cookies.Append("token", token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddHours(1)
		});

		return Ok(new { message = "Logged in" });
	}

	[HttpPost("logout")]
	[EnableRateLimiting("login")]
	public IActionResult LogOut()
	{
		Response.Cookies.Delete("token", new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax
		});
		return Ok(new { message = "Logged out" });
	}

	[HttpGet("verify")]
	public IActionResult Verify()
	{
		return Ok(new { message = "Valid" });
	}

	private string GenerateJwtToken(string username, int id, string role)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.Name, username),
			new Claim(ClaimTypes.Role, role),
			new Claim(ClaimTypes.NameIdentifier, id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var token = new JwtSecurityToken(
			issuer: _jwt.Issuer,
			audience: _jwt.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

}

