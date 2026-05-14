namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkingHoursController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorkingHoursController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpGet("{staffId}")]
    public async Task<IActionResult> Get(int staffId)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == staffId && s.BusinessId == GetBusinessId());
        if (staff == null) return NotFound();

        var hours = await _db.WorkingHours.Where(w => w.StaffId == staffId).ToListAsync();
        return Ok(hours);
    }

    [HttpPut("{staffId}")]
    public async Task<IActionResult> Update(int staffId, [FromBody] List<WorkingHours> hours)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == staffId && s.BusinessId == GetBusinessId());
        if (staff == null) return NotFound();

        var existing = await _db.WorkingHours.Where(w => w.StaffId == staffId).ToListAsync();
        _db.WorkingHours.RemoveRange(existing);

        foreach(var h in hours)
        {
            h.Id = 0;
            h.StaffId = staffId;
        }

        _db.WorkingHours.AddRange(hours);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
