namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Appointments;

public class StatusUpdateDto
{
    public string Status { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppointmentsController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? date, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        [FromQuery] string? status, 
        [FromQuery] int? staffId)
    {
        var query = _db.Appointments
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Where(a => a.BusinessId == GetBusinessId())
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(a => a.StartTime.Date == date.Value.Date);
            
        if (startDate.HasValue)
            query = query.Where(a => a.StartTime.Date >= startDate.Value.Date);
            
        if (endDate.HasValue)
            query = query.Where(a => a.StartTime.Date <= endDate.Value.Date);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        if (staffId.HasValue)
            query = query.Where(a => a.StaffId == staffId.Value);

        var appointments = await query.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            StaffId = a.StaffId,
            StaffName = a.Staff.Name,
            ServiceId = a.ServiceId,
            ServiceName = a.Service.Name,
            ServiceColor = a.Service.Color,
            ClientName = a.ClientName,
            ClientPhone = a.ClientPhone,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        }).ToListAsync();

        return Ok(appointments);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto request)
    {
        var validStatuses = new[] { "pending", "confirmed", "cancelled", "no_show" };
        if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status");

        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.BusinessId == GetBusinessId());
        if (appointment == null) return NotFound();

        appointment.Status = request.Status;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
