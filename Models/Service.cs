namespace server.Models;

public class Service
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int DurationMinutes { get; set; } = 30;
    public decimal? Price { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public bool IsActive { get; set; } = true;
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
