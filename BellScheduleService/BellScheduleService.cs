using AngleSharp;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DBHSBells.Services;

public class BellScheduleService
{
    private readonly HttpClient _httpClient;
    private readonly HtmlParser _parser;

    public BellScheduleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");
        _parser = new HtmlParser();
    }

    public async Task<List<Schedule>> GetBellSchedulesAsync()
    {
        var response = await _httpClient.GetAsync("https://dbhs.wvusd.org/apps/bell_schedules/");
        var html = await response.Content.ReadAsStringAsync();
        var document = await _parser.ParseDocumentAsync(html);

        var schedules = new List<Schedule>();

        var scheduleNodes = document.QuerySelectorAll("table.bell-schedule");

        foreach (var node in scheduleNodes)
        {
            var schedule = new Schedule
            {
                Title = node.QuerySelector("caption a").TextContent,
                Details = new List<ScheduleDetail>()
            };

            var detailNodes = node.QuerySelectorAll("tbody tr");
            foreach (var detailNode in detailNodes)
            {
                var description = detailNode.QuerySelector("td:nth-child(1)").TextContent;
                var startTime = detailNode.QuerySelector("td:nth-child(2)").TextContent;
                var endTime = detailNode.QuerySelector("td:nth-child(3)").TextContent;
                var length = detailNode.QuerySelector("td:nth-child(4)").TextContent;

                schedule.Details.Add(new ScheduleDetail
                {
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime,
                    Length = length
                });
            }

            schedules.Add(schedule);
        }

        return schedules;
    }

    
}