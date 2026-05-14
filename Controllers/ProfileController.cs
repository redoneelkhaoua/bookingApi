namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using System.Threading.Tasks;

public class UpdateBusinessProfileDto
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public string Timezone { get; set; } = "Africa/Casablanca";
    public string? Category { get; set; }
    public string? Description { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var business = await _db.Businesses.FindAsync(GetBusinessId());
        if (business == null) return NotFound();

        return Ok(new
        {
            business.Id,
            business.Name,
            business.Slug,
            business.Phone,
            business.Address,
            business.LogoUrl,
            business.Timezone,
            business.Category,
            business.Description
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateBusinessProfileDto request)
    {
        var business = await _db.Businesses.FindAsync(GetBusinessId());
        if (business == null) return NotFound();

        business.Name = request.Name;
        business.Phone = request.Phone;
        business.Address = request.Address;
        business.LogoUrl = request.LogoUrl;
        business.Timezone = request.Timezone;
        business.Category = request.Category;
        business.Description = request.Description;

        await _db.SaveChangesAsync();

        return Ok(business);
    }
}
