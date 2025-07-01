using SurveyApp.Core.Models;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.UI;

public sealed class ApplicationContext
{
    public Survey? CurrentSurvey { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public bool IsInSurveyMode { get; set; }
    public ISurveyBuilder? CurrentSurveyBuilder { get; set; }
    public Dictionary<string, object> Data { get; } = new();
}