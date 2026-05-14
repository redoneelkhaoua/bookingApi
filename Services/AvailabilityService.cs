namespace server.Services;

using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Availability;

public class AvailabilityService
{
    private readonly AppDbContext _db;

    public AvailabilityService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Core algorithm to get available slots for a given staff, service, and date.
    /// Filters out working hours, existing appointments, and blocked slots.
    /// </summary>
    public async Task<List<TimeSlot>> GetAvailableSlots(int staffId, int serviceId, DateTime date)
    {
        // 1. Get working hours for that day
        var dayOfWeek = (int)date.DayOfWeek;
        var workingHours = await _db.WorkingHours
            .FirstOrDefaultAsync(w => w.StaffId == staffId 
                                   && w.DayOfWeek == dayOfWeek);
        
        if (workingHours == null || workingHours.IsOff)
            return new List<TimeSlot>();

        // 2. Get service duration
        var service = await _db.Services.FindAsync(serviceId);
        if (service == null) return new List<TimeSlot>();

        var duration = service.DurationMinutes;

        // 3. Generate all slots
        var slots = new List<TimeSlot>();
        var current = date.Date + workingHours.StartTime;
        var end = date.Date + workingHours.EndTime;
        
        while (current.AddMinutes(duration) <= end)
        {
            slots.Add(new TimeSlot { Start = current, End = current.AddMinutes(duration) });
            current = current.AddMinutes(duration);
        }

        // 4. Get booked appointments
        var booked = await _db.Appointments
            .Where(a => a.StaffId == staffId 
                     && a.StartTime.Date == date.Date
                     && (a.Status == "pending" || a.Status == "confirmed"))
            .ToListAsync();

        // 5. Get blocked slots
        var blocked = await _db.BlockedSlots
            .Where(b => b.StaffId == staffId 
                     && b.StartTime.Date == date.Date)
            .ToListAsync();

        // 6. Filter out unavailable slots
        var now = DateTime.UtcNow;
        return slots.Where(slot =>
            slot.Start > now &&
            !booked.Any(b => slot.Start < b.EndTime && slot.End > b.StartTime) &&
            !blocked.Any(b => slot.Start < b.EndTime && slot.End > b.StartTime)
        ).ToList();
    }
}
