namespace server.Controllers;

using Microsoft.AspNetCore.Mvc;
using server.DTOs.Auth;
using server.Services;
using Microsoft.AspNetCore.Authorization;
using server.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var token = await _authService.LoginAsync(request);

        if (token == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(new { token });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto request, [FromServices] AppDbContext db)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Name = request.Name;
        user.PhotoUrl = request.PhotoUrl;
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Name, user.Email, user.PhotoUrl, user.Role });
    }

    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, [FromServices] AppDbContext db)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Current password is incorrect" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateUserProfileDto
{
    public string Name { get; set; } = null!;
    public string? PhotoUrl { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
