using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly ISurveyRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StatisticsService(ISurveyRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
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

    private int CalculateAverageAge(IEnumerable<Survey> surveys)
    {
        var today = _dateTimeProvider.Today;
        var ages = surveys.Select(s => CalculateAgeFromAnswers(s, today)).Where(age => age.HasValue).Select(age => age.Value);
        return ages.Any() ? (int)Math.Round(ages.Average()) : 0;
    }

    private int? CalculateAgeFromAnswers(Survey survey, DateTime referenceDate)
    {
        // Ищем дату рождения в ответах
        var dateKeys = new[] { "Дата рождения", "Date of birth", "Birthday" };
        
        foreach (var key in dateKeys)
        {
            if (survey.Answers.TryGetValue(key, out var dateStr) && 
                DateTime.TryParseExact(dateStr, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var birthDate))
            {
                var age = referenceDate.Year - birthDate.Year;
                if (birthDate.Date > referenceDate.AddYears(-age))
                    age--;
                return age;
            }
        }
        
        // Если не найдено в ответах, используем старое поле
        return survey.BirthDate != default ? survey.CalculateAge(referenceDate) : null;
    }

    private static Core.Enums.ProgrammingLanguage GetMostPopularLanguage(IEnumerable<Survey> surveys)
    {
        var languages = surveys.Select(GetLanguageFromAnswers).Where(lang => lang.HasValue).Select(lang => lang.Value);
        
        if (!languages.Any())
            return Core.Enums.ProgrammingLanguage.CSharp; // По умолчанию
            
        return languages
            .GroupBy(lang => lang)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private static Core.Enums.ProgrammingLanguage? GetLanguageFromAnswers(Survey survey)
    {
        // Ищем язык программирования в ответах
        var langKeys = new[] { "Язык программирования", "Programming language", "Language" };
        
        foreach (var key in langKeys)
        {
            if (survey.Answers.TryGetValue(key, out var langStr) && 
                Enum.TryParse<Core.Enums.ProgrammingLanguage>(langStr, true, out var language))
            {
                return language;
            }
        }
        
        // Если не найдено в ответах, используем старое поле
        return survey.Language != default ? survey.Language : null;
    }

    private static string GetMostExperiencedProgrammer(IEnumerable<Survey> surveys)
    {
        var surveysWithExperience = surveys
            .Select(s => new { Survey = s, Experience = GetExperienceFromAnswers(s), Name = GetNameFromAnswers(s) })
            .Where(x => x.Experience.HasValue && !string.IsNullOrWhiteSpace(x.Name))
            .ToList();
            
        if (!surveysWithExperience.Any())
            return "Не найдено";
            
        var mostExperienced = surveysWithExperience
            .OrderByDescending(x => x.Experience.Value)
            .First();
            
        return $"{mostExperienced.Name} ({mostExperienced.Experience} лет)";
    }

    private static int? GetExperienceFromAnswers(Survey survey)
    {
        // Ищем опыт работы в ответах
        var expKeys = new[] { "Опыт работы (лет)", "Experience", "Опыт программирования" };
        
        foreach (var key in expKeys)
        {
            if (survey.Answers.TryGetValue(key, out var expStr) && 
                int.TryParse(expStr, out var experience))
            {
                return experience;
            }
        }
        
        // Если не найдено в ответах, используем старое поле
        return survey.ExperienceYears != default ? survey.ExperienceYears : null;
    }

    private static string GetNameFromAnswers(Survey survey)
    {
        // Ищем имя в ответах
        var nameKeys = new[] { "ФИО", "Введите ваше имя", "Name", "Full Name" };
        
        foreach (var key in nameKeys)
        {
            if (survey.Answers.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }
        
        // Если не найдено в ответах, используем старое поле
        return !string.IsNullOrWhiteSpace(survey.FullName) ? survey.FullName : "Неизвестно";
    }
}