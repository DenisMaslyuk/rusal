using System.Text.RegularExpressions;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public sealed class PhoneValidationStrategy : IValidationStrategy<string>
{
    private static readonly Regex PhoneRegex = new(@"^(\+7|8)?[\s\-]?\(?[0-9]{3}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$", RegexOptions.Compiled);

    public ValidationResult Validate(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return ValidationResult.Failure("Номер телефона не может быть пустым");
        }

        if (!PhoneRegex.IsMatch(phoneNumber))
        {
            return ValidationResult.Failure("Неверный формат номера телефона");
        }

        return ValidationResult.Success();
    }
}