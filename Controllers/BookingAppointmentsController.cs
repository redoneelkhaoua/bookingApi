namespace server.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Appointments;
using server.Models;
using server.Services;

[ApiController]
[Route("api/booking/{slug}/appointments")]
public class BookingAppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AvailabilityService _availabilityService;
    private readonly WhatsAppService _whatsAppService;

    public BookingAppointmentsController(AppDbContext db, AvailabilityService availabilityService, WhatsAppService whatsAppService)
    {
        _db = db;
        _availabilityService = availabilityService;
        _whatsAppService = whatsAppService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string slug, [FromBody] CreateAppointmentDto dto)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Slug == slug);
        if (business == null) return NotFound("Business not found");

        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.BusinessId == business.Id);
        if (service == null) return NotFound("Service not found");

        var availableSlots = await _availabilityService.GetAvailableSlots(dto.StaffId, dto.ServiceId, dto.StartTime.Date);
        if (!availableSlots.Any(s => s.Start == dto.StartTime))
            return BadRequest("Slot is no longer available");

        var appointment = new Appointment
        {
            BusinessId = business.Id,
            StaffId = dto.StaffId,
            ServiceId = dto.ServiceId,
            ClientName = dto.ClientName,
            ClientPhone = dto.ClientPhone,
            StartTime = dto.StartTime,
            EndTime = dto.StartTime.AddMinutes(service.DurationMinutes),
            Status = "pending",
            Notes = dto.Notes,
            CancelToken = Guid.NewGuid().ToString()
        };

        _db.Appointments.Add(appointment);

        // Auto-upsert Client
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.BusinessId == business.Id && c.Phone == dto.ClientPhone);
        if (client == null)
        {
            client = new Client
            {
                BusinessId = business.Id,
                Name = dto.ClientName,
                Phone = dto.ClientPhone
            };
            _db.Clients.Add(client);
        }
        else
        {
            client.Name = dto.ClientName; // Update name just in case
        }

        await _db.SaveChangesAsync();

        // Reload appointment with relations for WhatsApp
        await _db.Entry(appointment).Reference(a => a.Service).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.Staff).LoadAsync();
        await _db.Entry(appointment).Reference(a => a.Business).LoadAsync();

        // Send WhatsApp confirmation
        await _whatsAppService.SendConfirmationAsync(appointment);

        return Created("", new { appointment.Id, appointment.StartTime, appointment.EndTime, appointment.CancelToken });
    }

    [HttpGet("{phone}")]
    public async Task<IActionResult> GetByPhone(string slug, string phone)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Slug == slug);
        if (business == null) return NotFound("Business not found");

        var appointments = await _db.Appointments
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Where(a => a.BusinessId == business.Id && a.ClientPhone == phone)
            .Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                StaffId = a.StaffId,
                StaffName = a.Staff.Name,
                ServiceId = a.ServiceId,
                ServiceName = a.Service.Name,
                ClientName = a.ClientName,
                ClientPhone = a.ClientPhone,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(string slug, int id)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Slug == slug);
        if (business == null) return NotFound("Business not found");

        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.BusinessId == business.Id);
        if (appointment == null) return NotFound();

        appointment.Status = "cancelled";
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
