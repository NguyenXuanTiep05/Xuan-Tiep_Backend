using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using Auth.src.Models;
using Microsoft.AspNetCore.RateLimiting;

namespace Auth.src.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{

	private readonly Admin _admin;
	private readonly Jwt _jwt;

	public AuthController(
		IOptions<Admin> admin,
		IOptions<Jwt> jwt)
	{
		_admin = admin.Value;
		_jwt = jwt.Value;
	}


	[HttpPost("login")]
	[EnableRateLimiting("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		// TODO: replace with real user validation (e.g. database lookup)
		if (!FixedTimeEquals(request.Username, _admin.Username) || !FixedTimeEquals(request.Password, _admin.Password))
			return Unauthorized(new { message = "Invalid credentials" });

		var token = GenerateJwtToken(request.Username);
		Response.Cookies.Append("token", token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict,
			Expires = DateTimeOffset.UtcNow.AddHours(1)
		});

		return Ok(new { message = "Logged in" });
	}

	[Authorize]
	[HttpPost("logout")]
	[EnableRateLimiting("login")]
	public IActionResult LogOut()
	{
		Response.Cookies.Delete("token", new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict
		});
		return Ok(new { message = "Logged out" });
	}

	[Authorize]
	[HttpGet("verify")]
	public IActionResult Verify()
	{

		return Ok(new { message = "Valid" });
	}

	private string GenerateJwtToken(string username)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.Name, username),
			new Claim(ClaimTypes.Role, "User"),
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

	private static bool FixedTimeEquals(string? a, string? b)
	{
		return CryptographicOperations.FixedTimeEquals(
			Encoding.UTF8.GetBytes(a ?? ""),
			Encoding.UTF8.GetBytes(b ?? ""));
	}
}

