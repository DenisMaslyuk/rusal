namespace SurveyApp.Core.Interfaces;

public interface IAppLogger
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogError(Exception exception, string message);
}