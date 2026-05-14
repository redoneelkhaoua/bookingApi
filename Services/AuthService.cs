namespace server.Services;

using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Auth;
using BCrypt.Net;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (user == null || !BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        return _jwtService.GenerateToken(user);
    }
}
