using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly ISurveyRepository _repository;

    public StatisticsService(ISurveyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SurveyStatistics>> CalculateStatisticsAsync()
    {
        try
        {
            var surveys = await _repository.GetAllAsync();
            var surveyList = surveys.ToList();
            
            if (!surveyList.Any())
            {
                return Result<SurveyStatistics>.Failure("Нет заполненных анкет для расчета статистики");
            }

            var averageAge = CalculateAverageAge(surveyList);
            var mostPopularLanguage = GetMostPopularLanguage(surveyList);
            var mostExperiencedProgrammer = GetMostExperiencedProgrammer(surveyList);

            var statistics = new SurveyStatistics
            {
                AverageAge = averageAge,
                MostPopularLanguage = mostPopularLanguage,
                MostExperiencedProgrammer = mostExperiencedProgrammer
            };

            return Result<SurveyStatistics>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<SurveyStatistics>.Failure($"Ошибка при расчете статистики: {ex.Message}");
        }
    }

    private static int CalculateAverageAge(IEnumerable<Survey> surveys)
    {
        var ages = surveys.Select(s => s.Age);
        return (int)Math.Round(ages.Average());
    }

    private static Core.Enums.ProgrammingLanguage GetMostPopularLanguage(IEnumerable<Survey> surveys)
    {
        return surveys
            .GroupBy(s => s.Language)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private static string GetMostExperiencedProgrammer(IEnumerable<Survey> surveys)
    {
        return surveys
            .OrderByDescending(s => s.ExperienceYears)
            .First()
            .FullName;
    }
}