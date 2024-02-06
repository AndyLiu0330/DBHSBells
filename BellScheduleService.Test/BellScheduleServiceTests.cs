using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace BellScheduleService.Test;

public class BellScheduleServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly BellScheduleService _bellScheduleService;

    public BellScheduleServiceTests()
    {
        var mockHttp = new MockHttpMessageHandler();
        string baseDir = AppContext.BaseDirectory;
        string projectRootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        string testDataPath = Path.Combine(projectRootPath, "TestData", "bell_schedules.html");
        var htmlContent = File.ReadAllText(testDataPath);
        mockHttp.When("https://dbhs.wvusd.org/apps/bell_schedules/")
            .Respond(HttpStatusCode.OK, "text/html", htmlContent);
        mockHttp.When("*")
            .Respond(HttpStatusCode.BadRequest);

        _httpClient = new HttpClient(mockHttp);
        _bellScheduleService = new BellScheduleService(_httpClient);
    }


    [Fact]
    public async Task GetBellSchedulesAsync_ReturnsExpectedResult_WhenResponseIsValid()
    {
        // Act
        var result = await _bellScheduleService.GetBellSchedulesAsync();

        // Assert
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        // var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        var expectedJson = File.ReadAllText(Path.Combine(GetProjectRootPath(), "TestData", "bell_schedules.json"));
        var expectedJsonObject = JsonDocument.Parse(expectedJson);
        var resultJsonObject = JsonDocument.Parse(json);

        // Check all schedules
        for (int i = 0; i < expectedJsonObject.RootElement.GetArrayLength(); i++)
        {
            var expectedSchedule = expectedJsonObject.RootElement[i];
            var resultSchedule = resultJsonObject.RootElement[i];

            Assert.Equal(expectedSchedule.GetProperty("Title").GetString(),
                resultSchedule.GetProperty("Title").GetString());

            // Check all details of each schedule
            for (int j = 0; j < expectedSchedule.GetProperty("Details").GetArrayLength(); j++)
            {
                var expectedDetail = expectedSchedule.GetProperty("Details")[j];
                var resultDetail = resultSchedule.GetProperty("Details")[j];

                Assert.Equal(expectedDetail.GetProperty("Description").GetString(),
                    resultDetail.GetProperty("Description").GetString());
                Assert.Equal(expectedDetail.GetProperty("StartTime").GetString(),
                    resultDetail.GetProperty("StartTime").GetString());
                Assert.Equal(expectedDetail.GetProperty("EndTime").GetString(),
                    resultDetail.GetProperty("EndTime").GetString());
                Assert.Equal(expectedDetail.GetProperty("Length").GetString(),
                    resultDetail.GetProperty("Length").GetString());
            }
        }
    }

    private static string GetProjectRootPath()
    {
        string baseDir = AppContext.BaseDirectory;
        string projectRootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        return projectRootPath;
    }

    [Fact]
    public async Task GetBellSchedulesAsync_ThrowsException_WhenResponseIsInvalid()
    {
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _bellScheduleService.GetBellSchedulesAsync());
    }
}