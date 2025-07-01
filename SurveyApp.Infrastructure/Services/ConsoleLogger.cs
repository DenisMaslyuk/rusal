using SurveyApp.Core.Interfaces;

namespace SurveyApp.Infrastructure.Services;

public sealed class ConsoleLogger : IAppLogger
{
    public void LogInformation(string message)
    {
        Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogError(Exception exception, string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        Console.WriteLine($"Exception: {exception.Message}");
        if (exception.StackTrace != null)
        {
            Console.WriteLine($"StackTrace: {exception.StackTrace}");
        }
    }
}