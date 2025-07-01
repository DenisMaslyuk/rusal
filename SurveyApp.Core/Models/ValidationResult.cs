using SurveyApp.Core.Common;

namespace SurveyApp.Core.Models;

public readonly struct ValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private ValidationResult(bool isValid, string errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true, string.Empty);
    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);

    public static implicit operator bool(ValidationResult result) => result.IsValid;
    public static implicit operator Result(ValidationResult result) => 
        result.IsValid ? Result.Success() : Result.Failure(result.ErrorMessage);
}