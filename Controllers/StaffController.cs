namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly AppDbContext _db;

    public StaffController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _db.Staff.Where(s => s.BusinessId == GetBusinessId() && s.IsActive).ToListAsync();
        return Ok(staff);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Staff staff)
    {
        staff.BusinessId = GetBusinessId();
        _db.Staff.Add(staff);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = staff.Id }, staff);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Staff staff)
    {
        var existing = await _db.Staff.FirstOrDefaultAsync(s => s.Id == id && s.BusinessId == GetBusinessId());
        if (existing == null) return NotFound();

        existing.Name = staff.Name;
        existing.Specialty = staff.Specialty;
        existing.PhotoUrl = staff.PhotoUrl;
        existing.IsActive = staff.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Staff.FirstOrDefaultAsync(s => s.Id == id && s.BusinessId == GetBusinessId());
        if (existing == null) return NotFound();

        existing.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
