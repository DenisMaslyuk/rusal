using System.Globalization;
using Microsoft.Extensions.Options;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public sealed class DateValidationStrategy : IValidationStrategy
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SurveySettings _settings;

    public DateValidationStrategy(IDateTimeProvider dateTimeProvider, IOptions<SurveySettings> settings)
    {
        _dateTimeProvider = dateTimeProvider;
        _settings = settings.Value;
    }

    public ValidationResult Validate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
        {
            return ValidationResult.Failure("Дата рождения не может быть пустой");
        }

        if (!DateTime.TryParseExact(dateStr, _settings.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return ValidationResult.Failure($"Дата должна быть в формате {_settings.DateFormat}");
        }

        var today = _dateTimeProvider.Today;
        
        if (date > today)
        {
            return ValidationResult.Failure("Дата рождения не может быть в будущем");
        }

        var minDate = today.AddYears(-_settings.MaxAge);
        var maxDate = today.AddYears(-_settings.MinAge);

        if (date < minDate)
        {
            return ValidationResult.Failure($"Дата рождения не может быть более {_settings.MaxAge} лет назад");
        }

        if (date > maxDate)
        {
            return ValidationResult.Failure($"Возраст должен быть не менее {_settings.MinAge} лет");
        }

        return ValidationResult.Success();
    }
}
