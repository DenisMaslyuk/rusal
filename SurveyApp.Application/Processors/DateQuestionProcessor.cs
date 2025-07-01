using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;
using SurveyApp.Application.Validators;

namespace SurveyApp.Application.Processors;

public sealed class DateQuestionProcessor : IQuestionProcessor
{
    private readonly DateValidationStrategy _dateValidator;

    public DateQuestionProcessor(DateValidationStrategy dateValidator)
    {
        _dateValidator = dateValidator;
    }

    public QuestionType SupportedType => QuestionType.Date;

    public ValidationResult ValidateAnswer(string answer, IQuestionDefinition question)
    {
        if (question.IsRequired && string.IsNullOrWhiteSpace(answer))
        {
            return ValidationResult.Failure("Дата не может быть пустой");
        }

        return _dateValidator.Validate(answer);
    }

    public string FormatAnswer(string answer, IQuestionDefinition question)
    {
        return answer.Trim();
    }

    public string GetPrompt(IQuestionDefinition question)
    {
        return question.Prompt + " (в формате дд.мм.гггг):";
    }
}