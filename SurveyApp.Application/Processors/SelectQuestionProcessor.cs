using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Processors;

public sealed class SelectQuestionProcessor : IQuestionProcessor
{
    public QuestionType SupportedType => QuestionType.Select;

    public ValidationResult ValidateAnswer(string answer, IQuestionDefinition question)
    {
        if (question.IsRequired && string.IsNullOrWhiteSpace(answer))
        {
            return ValidationResult.Failure("Выберите один из вариантов");
        }

        if (question.Options?.TryGetValue("Values", out var valuesObj) == true && valuesObj is string[] validValues)
        {
            if (!validValues.Contains(answer))
            {
                return ValidationResult.Failure($"Недопустимое значение. Доступные варианты: {string.Join(", ", validValues)}");
            }
        }

        return ValidationResult.Success();
    }

    public string FormatAnswer(string answer, IQuestionDefinition question)
    {
        return answer.Trim();
    }

    public string GetPrompt(IQuestionDefinition question)
    {
        var prompt = question.Prompt;
        
        if (question.Options?.TryGetValue("Values", out var valuesObj) == true && valuesObj is string[] values)
        {
            prompt += $" ({string.Join("/", values)})";
        }
        
        return prompt + ":";
    }
}