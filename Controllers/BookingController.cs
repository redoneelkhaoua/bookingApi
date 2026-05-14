namespace server.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBusinessInfo(string slug)
    {
        var business = await _db.Businesses
            .Include(b => b.Services.Where(s => s.IsActive))
            .Include(b => b.Staff.Where(s => s.IsActive))
            .FirstOrDefaultAsync(b => b.Slug == slug);

        if (business == null)
            return NotFound(new { message = "Business not found" });

        return Ok(new
        {
            business.Id,
            business.Name,
            business.Slug,
            business.LogoUrl,
            business.Phone,
            business.Address,
            business.Timezone,
            Services = business.Services.Select(s => new { s.Id, s.Name, s.DurationMinutes, s.Price, s.Color }),
            Staff = business.Staff.Select(s => new { s.Id, s.Name, s.Specialty, s.PhotoUrl })
        });
    }
}
