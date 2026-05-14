namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BlockedSlotsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BlockedSlotsController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BlockedSlot slot)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == slot.StaffId && s.BusinessId == GetBusinessId());
        if (staff == null) return NotFound();

        _db.BlockedSlots.Add(slot);
        await _db.SaveChangesAsync();

        return Created("", slot);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var slot = await _db.BlockedSlots
            .Include(b => b.Staff)
            .FirstOrDefaultAsync(b => b.Id == id && b.Staff.BusinessId == GetBusinessId());
            
        if (slot == null) return NotFound();

        _db.BlockedSlots.Remove(slot);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
