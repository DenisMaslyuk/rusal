using SurveyApp.Core.Enums;

namespace SurveyApp.Core.Models;

public sealed record Survey
{
    public string FullName { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public ProgrammingLanguage Language { get; init; }
    public int ExperienceYears { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Dictionary<string, string> Answers { get; init; } = new();

    public int CalculateAge(DateTime referenceDate)
    {
        var age = referenceDate.Year - BirthDate.Year;
        
        if (BirthDate.Date > referenceDate.AddYears(-age))
            age--;
            
        return age;
    }

}

public sealed record SurveyStatistics
{
    public int AverageAge { get; init; }
    public ProgrammingLanguage MostPopularLanguage { get; init; }
    public string MostExperiencedProgrammer { get; init; } = string.Empty;
}