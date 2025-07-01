using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Processors;

public sealed class NumberQuestionProcessor : IQuestionProcessor
{
    public QuestionType SupportedType => QuestionType.Number;

    public ValidationResult ValidateAnswer(string answer, IQuestionDefinition question)
    {
        if (question.IsRequired && string.IsNullOrWhiteSpace(answer))
        {
            return ValidationResult.Failure("Число не может быть пустым");
        }

        if (!string.IsNullOrWhiteSpace(answer) && !double.TryParse(answer, out var number))
        {
            return ValidationResult.Failure("Введите корректное число");
        }

        if (question.Options != null && double.TryParse(answer, out number))
        {
            if (question.Options.TryGetValue("Min", out var minObj) && 
                minObj is double min && number < min)
            {
                return ValidationResult.Failure($"Минимальное значение: {min}");
            }

            if (question.Options.TryGetValue("Max", out var maxObj) && 
                maxObj is double max && number > max)
            {
                return ValidationResult.Failure($"Максимальное значение: {max}");
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
        
        if (question.Options != null)
        {
            var constraints = new List<string>();
            
            if (question.Options.TryGetValue("Min", out var min))
                constraints.Add($"мин. {min}");
                
            if (question.Options.TryGetValue("Max", out var max))
                constraints.Add($"макс. {max}");
                
            if (constraints.Count > 0)
                prompt += $" ({string.Join(", ", constraints)})";
        }
        
        return prompt + ":";
    }
}