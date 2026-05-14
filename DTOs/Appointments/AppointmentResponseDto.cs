namespace server.DTOs.Appointments;

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = null!;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? ServiceColor { get; set; }
    public string ClientName { get; set; } = null!;
    public string ClientPhone { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
