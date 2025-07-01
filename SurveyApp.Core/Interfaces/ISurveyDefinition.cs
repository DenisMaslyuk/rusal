using SurveyApp.Core.Models;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyDefinition
{
    string SurveyType { get; }
    string DisplayName { get; }
    IReadOnlyList<IQuestionDefinition> Questions { get; }
}