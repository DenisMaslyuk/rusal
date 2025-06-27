namespace SurveyApp.Core.Models;

public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string errorMessage) => new() 
    { 
        IsValid = false, 
        ErrorMessage = errorMessage 
    };
}