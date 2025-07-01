using SurveyApp.Core.Models;

namespace SurveyApp.Core.Interfaces;

public interface IQuestionDefinition
{
    int Index { get; }
    string Prompt { get; }
    QuestionType Type { get; }
    bool IsRequired { get; }
    Dictionary<string, object>? Options { get; }
}