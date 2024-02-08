using System.Text.Json;

public class Schedule
{
    public string Title { get; set; }
    public string Info { get; set; }
    public List<ScheduleDetail> Details { get; set; } = new List<ScheduleDetail>();

    public int GetCurrentPeriod(DateTime now)
    {
        var currentPeriod = 0;
        // to modify the rules
        // 0. if now is before the first detail's start time, return -1
        // 1. if now is between the start and end time of a detail, return the currentPeriod
        // 2. if now is between the previous detail's end time and the current detail's start time, return the currentPeriod
        // 3. if now is after the last detail's end time, return -2
        
        if (now < DateTime.Parse(this.Details[0].StartTime))
        {
            return -1;
        }

        for (int i = 0; i < this.Details.Count; i++)
        {
            var start = DateTime.Parse(this.Details[i].StartTime);
            var end = DateTime.Parse(this.Details[i].EndTime);
            if (now >= start && now <= end)
            {
                return i;
            }

            if (i < this.Details.Count - 1)
            {
                var nextStart = DateTime.Parse(this.Details[i + 1].StartTime);
                if (now > end && now < nextStart)
                {
                    return i;
                }
            }
        }

        if (now > DateTime.Parse(this.Details[this.Details.Count - 1].EndTime))
        {
            return -2;
        }

        return -100;
    }
}