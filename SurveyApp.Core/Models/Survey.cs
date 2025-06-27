using SurveyApp.Core.Enums;

namespace SurveyApp.Core.Models;

public sealed record Survey
{
    public string FullName { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public ProgrammingLanguage Language { get; init; }
    public int ExperienceYears { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public int Age => CalculateAge();

    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - BirthDate.Year;
        
        if (BirthDate.Date > today.AddYears(-age))
            age--;
            
        return age;
    }

    public string GetFileName() => $"{FullName.Replace(" ", "_")}.txt";
}

public sealed record SurveyStatistics
{
    public int AverageAge { get; init; }
    public ProgrammingLanguage MostPopularLanguage { get; init; }
    public string MostExperiencedProgrammer { get; init; } = string.Empty;
}