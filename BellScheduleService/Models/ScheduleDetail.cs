using Microsoft.Maui.Storage;

public class ScheduleDetail
{
    public bool Enabled
    {
        get;
        set;
        // get => GetEnableFromPrefreence(Description);
    }

    public bool IsAPScience
    {
        get => Description.Contains("AP");
    }

    public string Description { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Length { get; set; }

    public bool GetEnableFromPrefreence(string textContent)
    {
        if (textContent.Contains("Period 0"))
        {
            return Preferences.Get("p0", false);
        }
        else if (textContent.Contains("Period 1"))
        {
            if (textContent.Contains("(AP SCIENCE only)"))
            {
                return Preferences.Get("p1", false);
            }

            return !Preferences.Get("p1", false);
        }
        else if (textContent.Contains("Period 6"))
        {
            if (textContent.Contains("(AP SCIENCE only)"))
            {
                return Preferences.Get("p6", false);
            }

            return !Preferences.Get("p6", false);
        }

        return true;
    }
}