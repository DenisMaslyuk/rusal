using System.Text.RegularExpressions;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Processors;

public sealed class PhoneQuestionProcessor : IQuestionProcessor
{
    private static readonly Regex PhoneRegex = new(@"^(\+7|8)[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$", 
        RegexOptions.Compiled);

    public QuestionType SupportedType => QuestionType.Phone;

    public ValidationResult ValidateAnswer(string answer, IQuestionDefinition question)
    {
        if (question.IsRequired && string.IsNullOrWhiteSpace(answer))
        {
            return ValidationResult.Failure("Номер телефона не может быть пустым");
        }

        if (!string.IsNullOrWhiteSpace(answer) && !PhoneRegex.IsMatch(answer))
        {
            return ValidationResult.Failure("Некорректный формат номера телефона. Используйте формат: +7(999)999-99-99 или 8-999-999-99-99");
        }

        return ValidationResult.Success();
    }

    public string FormatAnswer(string answer, IQuestionDefinition question)
    {
        return answer.Trim();
    }

    public string GetPrompt(IQuestionDefinition question)
    {
        return question.Prompt + " (например: +7(999)999-99-99):";
    }
}