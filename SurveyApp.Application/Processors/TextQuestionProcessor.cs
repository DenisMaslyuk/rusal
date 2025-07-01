using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Processors;

public sealed class TextQuestionProcessor : IQuestionProcessor
{
    public QuestionType SupportedType => QuestionType.Text;

    public ValidationResult ValidateAnswer(string answer, IQuestionDefinition question)
    {
        if (question.IsRequired && string.IsNullOrWhiteSpace(answer))
        {
            return ValidationResult.Failure("Поле не может быть пустым");
        }

        if (question.Options != null)
        {
            if (question.Options.TryGetValue("MinLength", out var minLengthObj) && 
                minLengthObj is int minLength && 
                answer.Length < minLength)
            {
                return ValidationResult.Failure($"Минимальная длина: {minLength} символов");
            }

            if (question.Options.TryGetValue("MaxLength", out var maxLengthObj) && 
                maxLengthObj is int maxLength && 
                answer.Length > maxLength)
            {
                return ValidationResult.Failure($"Максимальная длина: {maxLength} символов");
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
            
            if (question.Options.TryGetValue("MinLength", out var minLength))
                constraints.Add($"мин. {minLength} символов");
                
            if (question.Options.TryGetValue("MaxLength", out var maxLength))
                constraints.Add($"макс. {maxLength} символов");
                
            if (constraints.Count > 0)
                prompt += $" ({string.Join(", ", constraints)})";
        }
        
        return prompt + ":";
    }
}