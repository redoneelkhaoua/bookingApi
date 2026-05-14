namespace server.Models;

public class Staff
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Specialty { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<BlockedSlot> BlockedSlots { get; set; } = new List<BlockedSlot>();
}
