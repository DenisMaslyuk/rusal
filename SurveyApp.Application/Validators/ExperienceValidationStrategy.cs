using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public sealed class ExperienceValidationStrategy : IValidationStrategy<string>
{
    public ValidationResult Validate(string experienceStr)
    {
        if (string.IsNullOrWhiteSpace(experienceStr))
        {
            return ValidationResult.Failure("Опыт программирования не может быть пустым");
        }

        if (!int.TryParse(experienceStr, out var experience))
        {
            return ValidationResult.Failure("Опыт должен быть числом");
        }

        if (experience < 0)
        {
            return ValidationResult.Failure("Опыт не может быть отрицательным");
        }

        if (experience > 70)
        {
            return ValidationResult.Failure("Опыт не может быть более 70 лет");
        }

        return ValidationResult.Success();
    }
}