using SurveyApp.Core.Interfaces;

namespace SurveyApp.Core.Models;

public sealed class QuestionDefinition : IQuestionDefinition
{
    public int Index { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public QuestionType Type { get; init; }
    public bool IsRequired { get; init; } = true;
    public Dictionary<string, object>? Options { get; init; }
}