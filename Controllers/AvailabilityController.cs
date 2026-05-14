namespace server.Controllers;

using Microsoft.AspNetCore.Mvc;
using server.Services;
using server.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/booking/{slug}/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly AvailabilityService _availabilityService;
    private readonly AppDbContext _db;

    public AvailabilityController(AvailabilityService availabilityService, AppDbContext db)
    {
        _availabilityService = availabilityService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailability(string slug, [FromQuery] int staffId, [FromQuery] int serviceId, [FromQuery] DateTime date)
    {
        // Check if business exists
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Slug == slug);
        if (business == null)
            return NotFound(new { message = "Business not found" });

        var slots = await _availabilityService.GetAvailableSlots(staffId, serviceId, date);
        return Ok(slots);
    }
}
