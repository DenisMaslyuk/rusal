namespace SurveyApp.Core.Models;

public sealed class SurveySettings
{
    public string SurveyDirectory { get; set; } = "Анкеты";
    public int MaxAge { get; set; } = 120;
    public int MinAge { get; set; } = 0;
    public string DateFormat { get; set; } = "dd.MM.yyyy";
}
