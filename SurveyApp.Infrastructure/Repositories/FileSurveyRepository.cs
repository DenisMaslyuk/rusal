using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Infrastructure.Repositories;

public sealed class FileSurveyRepository : ISurveyRepository
{
    private readonly string _surveysDirectory;
    private readonly IFileNameService _fileNameService;
    private readonly IAppLogger _logger;
    private readonly SurveySettings _settings;

    public FileSurveyRepository(IFileNameService fileNameService, IAppLogger logger, IOptions<SurveySettings> settings)
    {
        _fileNameService = fileNameService;
        _logger = logger;
        _settings = settings.Value;
        _surveysDirectory = Path.Combine(Directory.GetCurrentDirectory(), _settings.SurveyDirectory);
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
        
        // Получаем имя из новой структуры Answers
        var fullName = GetFullNameFromAnswers(survey);
        var fileName = _fileNameService.GenerateFileName(fullName);
        
        if (!_fileNameService.IsValidFileName(fileName))
            throw new ArgumentException($"Generated filename is invalid: {fileName}");
            
        var filePath = Path.Combine(_surveysDirectory, fileName);
        
        if (!IsPathSecure(filePath))
            throw new UnauthorizedAccessException("File path is not secure");
            
        var content = FormatSurveyContent(survey);
        
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8).ConfigureAwait(false);
    }

    private string GetFullNameFromAnswers(Survey survey)
    {
        // Ищем имя в ответах (может быть под разными ключами)
        var nameKeys = new[] { "Введите ваше имя", "ФИО", "Имя", "Name" };
        
        foreach (var key in nameKeys)
        {
            if (survey.Answers.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }
        
        // Если не найдено, используем старые поля
        return !string.IsNullOrWhiteSpace(survey.FullName) ? survey.FullName : "Unknown";
    }

    public async Task<Survey?> FindAsync(string fileName)
    {
        if (!_fileNameService.IsValidFileName(fileName))
            throw new ArgumentException($"Invalid filename: {fileName}");
            
        var filePath = Path.Combine(_surveysDirectory, fileName);
        
        if (!IsPathSecure(filePath))
            throw new UnauthorizedAccessException("File path is not secure");
        
        if (!File.Exists(filePath))
            return null;
            
        var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);
        return ParseSurveyFromContent(content);
    }

    public async Task<IEnumerable<Survey>> GetAllAsync()
    {
        if (!Directory.Exists(_surveysDirectory))
            return Enumerable.Empty<Survey>();
            
        var files = Directory.GetFiles(_surveysDirectory, "*.txt");
        var surveys = new List<Survey>();
        
        var tasks = files.Select(async file =>
        {
            try
            {
                var content = await File.ReadAllTextAsync(file, Encoding.UTF8).ConfigureAwait(false);
                return ParseSurveyFromContent(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading file {file}");
                return null;
            }
        });
        
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        surveys.AddRange(results.Where(survey => survey != null)!);
        
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
        
        // Используем новую структуру Answers, если доступна
        if (survey.Answers.Any())
        {
            int index = 1;
            foreach (var answer in survey.Answers)
            {
                sb.AppendLine($"{index}. {answer.Key}: {answer.Value}");
                index++;
            }
        }
        else
        {
            // Fallback к старому формату
            sb.AppendLine($"1. ФИО: {survey.FullName}");
            sb.AppendLine($"2. Дата рождения: {survey.BirthDate:dd.MM.yyyy}");
            sb.AppendLine($"3. Любимый язык программирования: {survey.Language.ToDisplayString()}");
            sb.AppendLine($"4. Опыт программирования на указанном языке: {survey.ExperienceYears}");
            sb.AppendLine($"5. Мобильный телефон: {survey.PhoneNumber}");
        }
        
        sb.AppendLine($"Анкета заполнена: {survey.CreatedAt:dd.MM.yyyy}");
        
        return sb.ToString();
    }

    private static Survey? ParseSurveyFromContent(string content)
    {
        try
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length < 2) // Минимум 1 поле + дата создания
                return null;
            
            var answers = new Dictionary<string, string>();
            DateTime createdAt = DateTime.Now;
            
            // Парсим ответы
            foreach (var line in lines)
            {
                if (line.Contains("Анкета заполнена:"))
                {
                    var createdAtStr = ExtractValue(line, "Анкета заполнена:");
                    DateTime.TryParseExact(createdAtStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdAt);
                    continue;
                }
                
                // Парсим строки вида "1. Ключ: Значение"
                var match = System.Text.RegularExpressions.Regex.Match(line, @"^\d+\.\s*(.+?):\s*(.*)$");
                if (match.Success)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();
                    answers[key] = value;
                }
            }
            
            // Заполняем старые поля для обратной совместимости
            var legacyFields = ExtractLegacyFields(answers);
            
            var survey = new Survey
            {
                CreatedAt = createdAt,
                Answers = answers,
                FullName = legacyFields.FullName,
                BirthDate = legacyFields.BirthDate,
                Language = legacyFields.Language,
                ExperienceYears = legacyFields.ExperienceYears,
                PhoneNumber = legacyFields.PhoneNumber
            };
            
            return survey;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing survey content: {ex.Message}");
            return null;
        }
    }

    private static (string FullName, DateTime BirthDate, Core.Enums.ProgrammingLanguage Language, int ExperienceYears, string PhoneNumber) ExtractLegacyFields(Dictionary<string, string> answers)
    {
        var fullName = string.Empty;
        var birthDate = default(DateTime);
        var language = default(Core.Enums.ProgrammingLanguage);
        var experienceYears = 0;
        var phoneNumber = string.Empty;
        
        // Извлекаем ФИО
        var nameKeys = new[] { "ФИО", "Введите ваше имя", "Name" };
        foreach (var key in nameKeys)
        {
            if (answers.TryGetValue(key, out var name))
            {
                fullName = name;
                break;
            }
        }
        
        // Извлекаем дату рождения
        var dateKeys = new[] { "Дата рождения", "Date of birth" };
        foreach (var key in dateKeys)
        {
            if (answers.TryGetValue(key, out var dateStr) &&
                DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            {
                break;
            }
        }
        
        // Извлекаем язык программирования
        var langKeys = new[] { "Язык программирования", "Любимый язык программирования" };
        foreach (var key in langKeys)
        {
            if (answers.TryGetValue(key, out var langStr) &&
                ProgrammingLanguageExtensions.TryParse(langStr, out language))
            {
                break;
            }
        }
        
        // Извлекаем опыт
        var expKeys = new[] { "Опыт работы (лет)", "Опыт программирования на указанном языке" };
        foreach (var key in expKeys)
        {
            if (answers.TryGetValue(key, out var expStr) &&
                int.TryParse(expStr, out experienceYears))
            {
                break;
            }
        }
        
        // Извлекаем телефон
        var phoneKeys = new[] { "Номер телефона", "Мобильный телефон" };
        foreach (var key in phoneKeys)
        {
            if (answers.TryGetValue(key, out phoneNumber))
            {
                break;
            }
        }
        
        return (fullName, birthDate, language, experienceYears, phoneNumber);
    }

    private static string ExtractValue(string line, string prefix)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new ArgumentException("Line cannot be null or empty", nameof(line));
            
        if (line.Length < prefix.Length || !line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Line does not contain expected prefix: {prefix}");
            
        return line.Substring(prefix.Length).Trim();
    }

    private bool IsPathSecure(string filePath)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            var surveysFullPath = Path.GetFullPath(_surveysDirectory);
            
            return fullPath.StartsWith(surveysFullPath + Path.DirectorySeparatorChar) ||
                   fullPath.Equals(surveysFullPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}