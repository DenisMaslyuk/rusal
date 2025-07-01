using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public interface IValidationStrategy
{
    ValidationResult Validate(string value);
}