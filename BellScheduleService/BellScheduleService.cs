using AngleSharp;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;

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
        var document = await _parser.ParseDocumentAsync(await response.Content.ReadAsStringAsync());

        var schedules = new List<Schedule>();

        foreach (var node in document.QuerySelectorAll("table.bell-schedule"))
        {
            var schedule = new Schedule
            {
                Title = node.QuerySelector("caption a").TextContent,
                Info =  node.QuerySelector("caption span[data-qa='bell-schedule-info']")?.InnerHtml ?? string.Empty,
                Details = new List<ScheduleDetail>()
            };

            foreach (var detailNode in node.QuerySelectorAll("tbody tr"))
            {
                schedule.Details.Add(new ScheduleDetail
                {
                    Description = detailNode.QuerySelector("td:nth-child(1)")?.TextContent  ?? string.Empty ,
                    StartTime = detailNode.QuerySelector("td:nth-child(2)")?.TextContent  ?? string.Empty,
                    EndTime = detailNode.QuerySelector("td:nth-child(3)")?.TextContent ?? string.Empty,
                    Length = detailNode.QuerySelector("td:nth-child(4)")?.TextContent ?? string.Empty
                });
            }

            schedules.Add(schedule);
        }

        return schedules;
    }

    
}