namespace server.Models;

public class Appointment
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public int StaffId { get; set; }
    public Staff Staff { get; set; } = null!;
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public string ClientPhone { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public string? CancelToken { get; set; }
    public bool ReminderSent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
