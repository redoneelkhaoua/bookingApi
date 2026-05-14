namespace server.DTOs.Availability;

public class TimeSlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class AvailableSlotsDto
{
    public List<TimeSlot> Slots { get; set; } = new List<TimeSlot>();
}
