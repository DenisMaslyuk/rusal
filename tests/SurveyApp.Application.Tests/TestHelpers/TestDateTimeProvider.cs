using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Tests.TestHelpers;

public class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime Now { get; set; } = new(2024, 6, 15, 10, 30, 0);
    public DateTime Today { get; set; } = new(2024, 6, 15);
    public DateTime UtcNow { get; set; } = new(2024, 6, 15, 8, 30, 0, DateTimeKind.Utc);
}