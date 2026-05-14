namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _db.Services.Where(s => s.BusinessId == GetBusinessId() && s.IsActive).ToListAsync();
        return Ok(services);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Service service)
    {
        service.BusinessId = GetBusinessId();
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Service service)
    {
        var existing = await _db.Services.FirstOrDefaultAsync(s => s.Id == id && s.BusinessId == GetBusinessId());
        if (existing == null) return NotFound();

        existing.Name = service.Name;
        existing.DurationMinutes = service.DurationMinutes;
        existing.Price = service.Price;
        existing.Color = service.Color;
        existing.IsActive = service.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Services.FirstOrDefaultAsync(s => s.Id == id && s.BusinessId == GetBusinessId());
        if (existing == null) return NotFound();

        existing.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
