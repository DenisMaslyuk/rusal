using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class SaveCommand : ICommand
{
    private readonly ISurveyRepository _repository;
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;
    private readonly IAppLogger _logger;

    public string Name => "-save";
    public string Description => "Сохранить заполненную анкету";

    public SaveCommand(ISurveyRepository repository, IConsoleUI consoleUI, ApplicationContext context, IAppLogger logger)
    {
        _repository = repository;
        _consoleUI = consoleUI;
        _context = context;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        if (_context.CurrentSurvey == null)
        {
            _consoleUI.ShowError("Нет заполненной анкеты для сохранения. Сначала заполните анкету с помощью команды -new_profile");
            return Result.Failure("Нет заполненной анкеты");
        }

        try
        {
            // Получаем имя из новой структуры
            var fullName = GetFullNameFromSurvey(_context.CurrentSurvey);
            _logger.LogInformation($"Saving survey for {fullName}");
            await _repository.SaveAsync(_context.CurrentSurvey).ConfigureAwait(false);
            
            // Генерируем имя файла так же, как в FileNameService
            var firstName = ExtractFirstName(fullName);
            var sanitizedName = SanitizeFileName(firstName);
            var fileName = $"{sanitizedName}.txt";
            
            _consoleUI.ShowSuccess($"Анкета успешно сохранена в файл: {fileName}");
            _logger.LogInformation($"Survey saved successfully: {fileName}");
            
            _context.CurrentSurvey = null;
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save survey");
            _consoleUI.ShowError($"Ошибка при сохранении анкеты: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }

    private string GetFullNameFromSurvey(Survey survey)
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

    private string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Unknown";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Если есть части, берем вторую часть как имя (Фамилия Имя Отчество)
        // Если частей меньше 2, берем первую часть
        return parts.Length >= 2 ? parts[1] : parts[0];
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Unknown";

        var sanitized = fileName;
        var invalidChars = Path.GetInvalidFileNameChars()
            .Concat(new[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' })
            .ToArray();
        
        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        sanitized = sanitized.Replace(" ", "_");
        
        if (sanitized.Length > 50)
            sanitized = sanitized[..50];

        return string.IsNullOrEmpty(sanitized) ? "Unknown" : sanitized;
    }
}