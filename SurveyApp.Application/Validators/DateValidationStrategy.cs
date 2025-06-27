using System.Globalization;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public sealed class DateValidationStrategy : IValidationStrategy<string>
{
    public ValidationResult Validate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
        {
            return ValidationResult.Failure("Дата рождения не может быть пустой");
        }

        if (!DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return ValidationResult.Failure("Дата должна быть в формате ДД.ММ.ГГГГ");
        }

        var minAge = DateTime.Today.AddYears(-120);
        var maxAge = DateTime.Today.AddYears(-16);

        if (date < minAge)
        {
            return ValidationResult.Failure("Дата рождения не может быть более 120 лет назад");
        }

        if (date > maxAge)
        {
            return ValidationResult.Failure("Возраст должен быть не менее 16 лет");
        }

        return ValidationResult.Success();
    }
}