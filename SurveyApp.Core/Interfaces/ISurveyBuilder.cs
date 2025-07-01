using SurveyApp.Core.Models;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyBuilder
{
    Survey BuildSurvey();
    ValidationResult SetAnswer(int questionIndex, string answer);
    ValidationResult ValidateCompleteness();
    (int completed, int total, List<string> answeredFields, List<string> missingFields) GetProgress();
    string GetCurrentAnswer(int questionIndex);
    bool HasAnswer(int questionIndex);
    string GetQuestionPrompt(int questionIndex);
    int GetQuestionCount();
    ValidationResult ValidateAnswer(int questionIndex, string answer);
}