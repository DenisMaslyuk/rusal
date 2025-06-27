using SurveyApp.Core.Models;

namespace SurveyApp.Application.Validators;

public interface IValidationStrategy<in T>
{
    ValidationResult Validate(T value);
}