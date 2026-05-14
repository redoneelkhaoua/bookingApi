namespace server.Models;

public class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Timezone { get; set; } = "Africa/Casablanca";
    public string? Category { get; set; }      // e.g. Clinique, Dentiste, Salon
    public string? Description { get; set; }   // short bio shown on directory
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
}
