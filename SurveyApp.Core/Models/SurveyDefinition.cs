using SurveyApp.Core.Interfaces;

namespace SurveyApp.Core.Models;

public sealed class SurveyDefinition : ISurveyDefinition
{
    public string SurveyType { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<IQuestionDefinition> Questions { get; init; } = Array.Empty<IQuestionDefinition>();
}