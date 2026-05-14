namespace server.Data;

using Microsoft.EntityFrameworkCore;
using server.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Business> Businesses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<BlockedSlot> BlockedSlots { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Business>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Service>()
            .Property(s => s.Price)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Client>()
            .HasIndex(c => new { c.BusinessId, c.Phone })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.CancelToken)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Business)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Staff)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Client>()
            .HasOne(c => c.Business)
            .WithMany(b => b.Clients)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<Business>().HasData(
            new Business
            {
                Id = 1,
                Name = "Demo Clinic",
                Slug = "demo-clinic",
                Phone = "+212600000000",
                Address = "123 Demo Street, Casablanca",
                Timezone = "Africa/Casablanca",
                Category = "Santé",
                Description = "Une clinique moderne offrant des soins de qualité supérieure avec une équipe de spécialistes dévoués.",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                BusinessId = 1,
                Name = "Admin User",
                Email = "admin@democlinic.com",
                PasswordHash = "$2a$11$0n.e.k3Ym9F04rA.r05kWeP3D6bB.GZ/vS/lS8X7B.B/t.s0z3/J6", // admin123
                Role = "admin",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // ── Services ──────────────────────────────────────────────────────
        modelBuilder.Entity<Service>().HasData(
            new Service
            {
                Id = 1, BusinessId = 1,
                Name = "General Consultation",
                DurationMinutes = 30,
                Price = 200,
                Color = "#2563EB",
                IsActive = true
            },
            new Service
            {
                Id = 2, BusinessId = 1,
                Name = "Follow-up Visit",
                DurationMinutes = 20,
                Price = 100,
                Color = "#10b981",
                IsActive = true
            },
            new Service
            {
                Id = 3, BusinessId = 1,
                Name = "Complete Check-up",
                DurationMinutes = 60,
                Price = 500,
                Color = "#f59e0b",
                IsActive = true
            }
        );

        // ── Staff ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Staff>().HasData(
            new Staff
            {
                Id = 1, BusinessId = 1,
                Name = "Karim Benali",
                Specialty = "General Practitioner",
                PhotoUrl = null,
                IsActive = true
            },
            new Staff
            {
                Id = 2, BusinessId = 1,
                Name = "Sara Idrissi",
                Specialty = "Internal Medicine",
                PhotoUrl = null,
                IsActive = true
            }
        );

        // ── Working Hours (Mon–Sat, 09:00–18:00 for both doctors) ─────────
        // DayOfWeek: 1=Mon 2=Tue 3=Wed 4=Thu 5=Fri 6=Sat
        var workingDays = new[] { 1, 2, 3, 4, 5, 6 };
        var whId = 1;
        foreach (var staffId in new[] { 1, 2 })
        {
            foreach (var day in workingDays)
            {
                modelBuilder.Entity<WorkingHours>().HasData(new WorkingHours
                {
                    Id        = whId++,
                    StaffId   = staffId,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime   = new TimeSpan(18, 0, 0),
                    IsOff     = false
                });
            }
        }

        // ── System Settings ───────────────────────────────────────────────
        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting
            {
                Id = 1,
                Key = "AzureBlobConnectionString",
                Value = "DefaultEndpointsProtocol=https;AccountName=sevenrayfi;AccountKey=YOUR_AZURE_ACCOUNT_KEY;EndpointSuffix=core.windows.net"
            }
        );
    }
}
