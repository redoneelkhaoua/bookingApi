namespace server.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using server.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ReminderScheduler : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ReminderScheduler> _logger;

    public ReminderScheduler(IServiceProvider services, ILogger<ReminderScheduler> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("ReminderScheduler started.");
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SendDueReminders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing SendDueReminders.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }

    private async Task SendDueReminders()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var whatsapp = scope.ServiceProvider.GetRequiredService<WhatsAppService>();

        var windowStart = DateTime.UtcNow.AddHours(23);
        var windowEnd = DateTime.UtcNow.AddHours(25);

        var dueAppointments = await db.Appointments
            .Where(a => a.StartTime >= windowStart
                     && a.StartTime <= windowEnd
                     && !a.ReminderSent
                     && (a.Status == "pending" || a.Status == "confirmed"))
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Include(a => a.Business)
            .ToListAsync();

        if (dueAppointments.Any())
        {
            _logger.LogInformation($"Found {dueAppointments.Count} appointments needing reminders.");
            
            foreach (var appt in dueAppointments)
            {
                await whatsapp.SendReminderAsync(appt);
                appt.ReminderSent = true;
            }

            await db.SaveChangesAsync();
        }
    }
}
