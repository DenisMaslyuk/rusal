using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public sealed class FullNameValidationStrategy : IValidationStrategy
{
    public ValidationResult Validate(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ValidationResult.Failure("ФИО не может быть пустым");
        }

        if (fullName.Length < 2)
        {
            return ValidationResult.Failure("ФИО слишком короткое");
        }

        if (fullName.Length > 100)
        {
            return ValidationResult.Failure("ФИО слишком длинное");
        }

        return ValidationResult.Success();
    }
}