using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // 從 config 讀取帳號清單
        var users = _config.GetSection("Users").Get<List<UserConfig>>() ?? [];
        var matched = users.FirstOrDefault(u =>
            u.Username == request.Username && u.Password == request.Password);

        if (matched is null)
            return Unauthorized(ApiResponse<LoginResponse>.Fail("帳號或密碼錯誤"));

        var jwt = _config.GetSection("JwtSettings");
        var secretKey = jwt["SecretKey"]!;
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;
        var expMinutes = int.Parse(jwt["ExpirationMinutes"] ?? "480");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: [
                new Claim(ClaimTypes.Name, matched.Username),
                new Claim("displayName", matched.DisplayName)
            ],
            expires: expiresAt,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(ApiResponse<LoginResponse>.Ok(
            new LoginResponse(tokenString, matched.Username, matched.DisplayName, expiresAt)));
    }
}

internal record UserConfig(string Username, string Password, string DisplayName);
