using System.Globalization;
using System.Text;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Infrastructure.Repositories;

public sealed class FileSurveyRepository : ISurveyRepository
{
    private readonly string _surveysDirectory;

    public FileSurveyRepository()
    {
        _surveysDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Анкеты");
    }

    public void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_surveysDirectory))
        {
            Directory.CreateDirectory(_surveysDirectory);
        }
    }

    public async Task SaveAsync(Survey survey)
    {
        EnsureDirectoryExists();
        
        var fileName = survey.GetFileName();
        var filePath = Path.Combine(_surveysDirectory, fileName);
        
        var content = FormatSurveyContent(survey);
        
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
    }

    public async Task<Survey?> FindAsync(string fileName)
    {
        var filePath = Path.Combine(_surveysDirectory, fileName);
        
        if (!File.Exists(filePath))
            return null;
            
        var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        return ParseSurveyFromContent(content);
    }

    public async Task<IEnumerable<Survey>> GetAllAsync()
    {
        if (!Directory.Exists(_surveysDirectory))
            return Enumerable.Empty<Survey>();
            
        var files = Directory.GetFiles(_surveysDirectory, "*.txt");
        var surveys = new List<Survey>();
        
        foreach (var file in files)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file, Encoding.UTF8);
                var survey = ParseSurveyFromContent(content);
                if (survey != null)
                {
                    surveys.Add(survey);
                }
            }
            catch
            {
                continue;
            }
        }
        
        return surveys;
    }

    public async Task<IEnumerable<Survey>> GetTodayAsync()
    {
        var allSurveys = await GetAllAsync();
        var today = DateTime.Today;
        
        return allSurveys.Where(s => s.CreatedAt.Date == today);
    }

    public async Task<bool> DeleteAsync(string fileName)
    {
        var filePath = Path.Combine(_surveysDirectory, fileName);
        
        if (!File.Exists(filePath))
            return false;
            
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetFileNamesAsync()
    {
        if (!Directory.Exists(_surveysDirectory))
            return Enumerable.Empty<string>();
            
        var files = Directory.GetFiles(_surveysDirectory, "*.txt");
        return files.Select(Path.GetFileName).Where(name => !string.IsNullOrEmpty(name))!;
    }

    private static string FormatSurveyContent(Survey survey)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"1. ФИО: {survey.FullName}");
        sb.AppendLine($"2. Дата рождения: {survey.BirthDate:dd.MM.yyyy}");
        sb.AppendLine($"3. Любимый язык программирования: {survey.Language.ToDisplayString()}");
        sb.AppendLine($"4. Опыт программирования на указанном языке: {survey.ExperienceYears}");
        sb.AppendLine($"5. Мобильный телефон: {survey.PhoneNumber}");
        sb.AppendLine($"Анкета заполнена: {survey.CreatedAt:dd.MM.yyyy}");
        
        return sb.ToString();
    }

    private static Survey? ParseSurveyFromContent(string content)
    {
        try
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            var fullName = ExtractValue(lines[0], "1. ФИО:");
            var birthDateStr = ExtractValue(lines[1], "2. Дата рождения:");
            var languageStr = ExtractValue(lines[2], "3. Любимый язык программирования:");
            var experienceStr = ExtractValue(lines[3], "4. Опыт программирования на указанном языке:");
            var phoneNumber = ExtractValue(lines[4], "5. Мобильный телефон:");
            var createdAtStr = ExtractValue(lines[5], "Анкета заполнена:");
            
            if (!DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate))
                return null;
                
            if (!ProgrammingLanguageExtensions.TryParse(languageStr, out var language))
                return null;
                
            if (!int.TryParse(experienceStr, out var experience))
                return null;
                
            if (!DateTime.TryParseExact(createdAtStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdAt))
                return null;
            
            return new Survey
            {
                FullName = fullName,
                BirthDate = birthDate,
                Language = language,
                ExperienceYears = experience,
                PhoneNumber = phoneNumber,
                CreatedAt = createdAt
            };
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractValue(string line, string prefix)
    {
        return line.Substring(prefix.Length).Trim();
    }
}