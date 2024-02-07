using System.Text.Json;

public class Schedule
{
    public string Title { get; set; }
    public List<ScheduleDetail> Details { get; set; } = new List<ScheduleDetail>();

    public int GetCurrentPeriod(DateTime now)
    {
        var currentPeriod = 0;
        foreach (var detail in this.Details)
        {
            var start = DateTime.Parse(detail.StartTime);
            var end = DateTime.Parse(detail.EndTime);
            if (now >= start && now <= end)
            {
                return currentPeriod;
            }

            currentPeriod++;
        }

        return -1;
    }
}