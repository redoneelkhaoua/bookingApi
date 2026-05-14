namespace server.DTOs.Appointments;

public class CreateAppointmentDto
{
    public int StaffId { get; set; }
    public int ServiceId { get; set; }
    public string ClientName { get; set; } = null!;
    public string ClientPhone { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public string? Notes { get; set; }
}
