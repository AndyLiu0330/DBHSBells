using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace DBHSBells.Services.Test;

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
    
    // test for GetCurrentPeriod, use the now and expected result as input for the fact
    // 1. now is before the first detail's start time
    // 2. now is after the last detail's end time
    // 3. now is Saturday or Sunday
    // 4. now is between the start and end time of a detail
    // 5. now is between the previous detail's end time and the current detail's start time
    
    [Theory]
    [InlineData("2022-01-01 07:00:00", -1)]
    [InlineData("2022-01-01 15:00:00", -2)]
    [InlineData("2022-01-01 12:00:00", -3)]  
    public async Task GetCurrentPeriod(string nowString, int expectedResult)
    {
        // Arrange
        var now = DateTime.Parse(nowString);
        var result = await _bellScheduleService.GetBellSchedulesAsync();

        // Act
        var period = result[0].GetCurrentPeriod(now);

        // Assert
        Assert.Equal(expectedResult, period);
    }
}