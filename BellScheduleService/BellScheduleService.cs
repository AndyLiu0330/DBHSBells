using AngleSharp;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Maui.Storage;

namespace DBHSBells.Services;

/// <summary>
/// Service for retrieving bell schedules.
/// </summary>
public class BellScheduleService
{
    private readonly HttpClient _httpClient;
    private readonly HtmlParser _parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="BellScheduleService"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient used for making HTTP requests.</param>
    public BellScheduleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");
        _parser = new HtmlParser();
    }

    /// <summary>
    /// Retrieves the bell schedules from the school website.
    /// </summary>
    /// <param name="forceRefresh">If true, ignores any cached schedules and fetches the schedules from the website. If false and there are cached schedules, uses the cached schedules.</param>
    public async Task<List<Schedule>> GetBellSchedulesAsync(bool forceRefresh = false)
    {
        var schedules = new List<Schedule>();
        IHtmlDocument document;

        // check if Preferences has the bell_schedules
        if (Preferences.ContainsKey("bell_schedules") && !forceRefresh)
        {
            var filePath = Preferences.Get("bell_schedules", string.Empty);
            var fileContent = await File.ReadAllTextAsync(filePath);
            document = await _parser.ParseDocumentAsync(fileContent);
        }
        else
        {
            var response = await _httpClient.GetAsync("https://dbhs.wvusd.org/apps/bell_schedules/");
            var htmlContent = await response.Content.ReadAsStringAsync();
            document = await _parser.ParseDocumentAsync(htmlContent);

            // Save the HTML content to a file
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "bell_schedules.html");
            await File.WriteAllTextAsync(filePath, htmlContent);

            // Save the file path to Preferences
            Preferences.Set("bell_schedules", filePath);
        }

        foreach (var node in document.QuerySelectorAll("table.bell-schedule"))
        {
            var schedule = new Schedule
            {
                Title = node.QuerySelector("caption a").TextContent,
                Info = node.QuerySelector("caption span[data-qa='bell-schedule-info']")?.InnerHtml ?? string.Empty,
                Details = new List<ScheduleDetail>()
            };

            foreach (var detailNode in node.QuerySelectorAll("tbody tr"))
            {
                schedule.Details.Add(new ScheduleDetail
                {
                    Description = detailNode.QuerySelector("td:nth-child(1)")?.TextContent ?? string.Empty,
                    StartTime = detailNode.QuerySelector("td:nth-child(2)")?.TextContent ?? string.Empty,
                    EndTime = detailNode.QuerySelector("td:nth-child(3)")?.TextContent ?? string.Empty,
                    Length = detailNode.QuerySelector("td:nth-child(4)")?.TextContent ?? string.Empty,
                    // Enabled = GetEnableFromPrefreence(detailNode.QuerySelector("td:nth-child(1)")?.TextContent ??
                    // string.Empty),
                });
            }

            schedule.SetEnableForDetailsType();
            schedules.Add(schedule);
        }

        return schedules;
    }
}