namespace server.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CancelController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly WhatsAppService _whatsAppService;

    public CancelController(AppDbContext db, WhatsAppService whatsAppService)
    {
        _db = db;
        _whatsAppService = whatsAppService;
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> CancelAppointment(string token)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Include(a => a.Business)
            .FirstOrDefaultAsync(a => a.CancelToken == token);

        if (appointment == null) return NotFound("Invalid or expired cancellation link.");
        if (appointment.Status == "cancelled") return BadRequest("Appointment is already cancelled.");

        appointment.Status = "cancelled";
        await _db.SaveChangesAsync();

        await _whatsAppService.SendCancellationAsync(appointment);

        return Ok(new { message = "Appointment successfully cancelled." });
    }
}
