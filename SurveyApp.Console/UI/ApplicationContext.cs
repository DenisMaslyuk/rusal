using SurveyApp.Application.Builders;
using SurveyApp.Core.Models;

namespace SurveyApp.Console.UI;

public sealed class ApplicationContext
{
    public Survey? CurrentSurvey { get; set; }
    public SurveyBuilder SurveyBuilder { get; } = new();
    public int CurrentQuestionIndex { get; set; }
    public bool IsInSurveyMode { get; set; }
    public Dictionary<string, object> Data { get; } = new();
}