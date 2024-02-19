using System.Text.Json;
using Microsoft.Maui.Storage;

public class Schedule
{
    public string Title { get; set; }
    public string Info { get; set; }
    public List<ScheduleDetail> Details { get; set; } = new List<ScheduleDetail>();

    public void SetEnableForDetailsType()
    {
        foreach (var detail in this.Details)
        {
            detail.Enabled = true;
        }

        // try select the p0 detail
        var p0 = this.Details.FirstOrDefault(d => d.Description.Contains("Period 0"));
        if (p0 != null)
        {
            p0.Enabled = Preferences.Get("p0", false);
        }

        SetPeriodEnable("1");
        SetPeriodEnable("6");
    }

    private void SetPeriodEnable(string period)
    {
        var pList = this.Details.Where(d => d.Description.Contains($"Period {period}"));

        if (pList.Count() == 2)
        {
            foreach (var p in pList)
            {
                if (p.IsAPScience)
                    p.Enabled = Preferences.Get($"p{period}", false);
                else
                    p.Enabled = !Preferences.Get($"p{period}", false);
            }
        }
        else if (pList.Count() == 1)
        {
            pList.First().Enabled = true;
        }
    }

    public int GetCurrentPeriod(DateTime now)
    {
        var currentPeriod = 0;
        var nowTime = now.TimeOfDay;
        // to modify the rules
        // 0. if now is before the first detail's start time, return -1
        //    if now is after the last detail's end time, return -2
        //    if now is Saturday or Sunday, return -3
        // 1. if now is between the start and end time of a detail, return the currentPeriod
        // 2. if now is between the previous detail's end time and the current detail's start time, return the currentPeriod
        
        if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
        {
            return -3;
        }

        if (nowTime < DateTime.Parse(this.Details[0].StartTime).TimeOfDay)
        {
            return -1;
        }
        
        if (nowTime > DateTime.Parse(this.Details[this.Details.Count - 1].EndTime).TimeOfDay)
        {
            return -2;
        }

        for (int i = 0; i < this.Details.Count; i++)
        {
            // if Enabled is false, skip
            if (!this.Details[i].Enabled)
            {
                continue;
            }

            var start = DateTime.Parse(this.Details[i].StartTime).TimeOfDay;
            var end = DateTime.Parse(this.Details[i].EndTime).TimeOfDay;
            if (nowTime < start)
            {
                if (Details[i - 1].Enabled)
                    return i - 1;
                else
                    return i - 2;
            }

            if (nowTime >= start && nowTime <= end)
            {
                return i;
            }

            // if (i < this.Details.Count - 1)
            // {
            //     var nextStart = DateTime.Parse(this.Details[i + 1].StartTime).TimeOfDay;
            //     if (nowTime > end && nowTime < nextStart)
            //     {
            //         return i;
            //     }
            // }
        }



        return -100;
    }
}