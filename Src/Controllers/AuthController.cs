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

		var token = Authentication.GenerateJwtToken(request.Username, id ?? -1, role!, _jwt);
		Authentication.SaveRefreshToken(_connStr.Default!, token,request.Username);
		Response.Cookies.Append("token", token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddDays(30)
		});

		return Ok(new { message = "Logged in" });
	}
	[AllowAnonymous]
	[HttpPost("refresh")]
	[EnableRateLimiting("login")]
	public IActionResult Refresh()
	{
		var oldToken = Request.Cookies["token"];
		Response.Cookies.Delete("token", new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax
		});
		if (oldToken is null) 
			return Unauthorized(new { message = "Login expired" });
		try
		{
		    Users.GetUserData(_connStr.Default!, oldToken, out int? id, out string? role, out string? username);
		    var newToken = Authentication.GenerateJwtToken(username!, id ?? -1, role!, _jwt);
		    Authentication.UpdateRefreshToken(_connStr.Default!, oldToken, newToken);
		    Response.Cookies.Append("token", newToken, new CookieOptions
			{
			    HttpOnly = true,
			    Secure = true,
			    SameSite = SameSiteMode.Lax,
			    Expires = DateTimeOffset.UtcNow.AddDays(30)
			});

		}
		catch
		{
		    return Unauthorized(new { message = "Login expired" });
		}

		return Ok(new { message = "Token refreshened" });

	}

	[HttpPost("logout")]
	[EnableRateLimiting("login")]
	public IActionResult LogOut()
	{
		var token = Request.Cookies["token"]!;
		Response.Cookies.Delete("token", new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax
		});

		try
		{
			Authentication.DeleteRefreshToken(_connStr.Default!, token);
		}
		catch
		{
			return Unauthorized( new {message = "Login expired"} );
		}
		return Ok(new { message = "Logged out" });
	}

	[HttpGet("verify")]
	public IActionResult Verify()
	{
		return Ok(new { message = "Valid" });
	}

}

