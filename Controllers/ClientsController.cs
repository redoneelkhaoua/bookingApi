namespace server.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using System.Linq;
using System.Threading.Tasks;

public class UpdateClientNotesDto
{
    public string? Notes { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClientsController(AppDbContext db)
    {
        _db = db;
    }

    private int GetBusinessId() => (int)HttpContext.Items["BusinessId"]!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _db.Clients
            .Where(c => c.BusinessId == GetBusinessId())
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Phone,
                c.Notes,
                c.CreatedAt,
                TotalAppointments = _db.Appointments.Count(a => a.ClientPhone == c.Phone && a.BusinessId == c.BusinessId)
            })
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _db.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == GetBusinessId());

        if (client == null) return NotFound();

        var appointments = await _db.Appointments
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.ClientPhone == client.Phone && a.BusinessId == GetBusinessId())
            .OrderByDescending(a => a.StartTime)
            .Select(a => new
            {
                a.Id,
                ServiceName = a.Service.Name,
                StaffName = a.Staff.Name,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Notes
            })
            .ToListAsync();

        return Ok(new
        {
            client.Id,
            client.Name,
            client.Phone,
            client.Notes,
            client.CreatedAt,
            Appointments = appointments
        });
    }

    [HttpPut("{id}/notes")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateClientNotesDto request)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == GetBusinessId());
        if (client == null) return NotFound();

        client.Notes = request.Notes;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
