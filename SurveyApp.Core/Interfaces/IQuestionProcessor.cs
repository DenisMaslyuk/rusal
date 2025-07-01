using SurveyApp.Core.Models;

namespace SurveyApp.Core.Interfaces;

public interface IQuestionProcessor
{
    QuestionType SupportedType { get; }
    ValidationResult ValidateAnswer(string answer, IQuestionDefinition question);
    string FormatAnswer(string answer, IQuestionDefinition question);
    string GetPrompt(IQuestionDefinition question);
}