namespace SurveyApp.Core.Interfaces;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime Today { get; }
    DateTime UtcNow { get; }
}

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.Today;
    public DateTime UtcNow => DateTime.UtcNow;
}