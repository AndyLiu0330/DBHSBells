using System.Text.Json;

public class Schedule
{
    public string Title { get; set; }
    public List<ScheduleDetail> Details { get; set; } = new List<ScheduleDetail>();
}