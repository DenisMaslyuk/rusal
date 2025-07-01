using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class FindCommand : ICommand
{
    private readonly ISurveyRepository _repository;
    private readonly IConsoleUI _consoleUI;

    public string Name => "-find";
    public string Description => "Найти анкету и показать данные анкеты в консоль";

    public FindCommand(ISurveyRepository repository, IConsoleUI consoleUI)
    {
        _repository = repository;
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            _consoleUI.ShowError("Укажите имя файла анкеты");
            return Result.Failure("Не указано имя файла");
        }

        var fileName = args[0];
        
        try
        {
            var survey = await _repository.FindAsync(fileName).ConfigureAwait(false);
            
            if (survey == null)
            {
                _consoleUI.ShowError($"Анкета с именем файла '{fileName}' не найдена");
                return Result.Failure("Анкета не найдена");
            }

            _consoleUI.WriteLine($"Данные анкеты '{fileName}':");
            _consoleUI.WriteLine($"1. ФИО: {survey.FullName}");
            _consoleUI.WriteLine($"2. Дата рождения: {survey.BirthDate:dd.MM.yyyy}");
            _consoleUI.WriteLine($"3. Любимый язык программирования: {survey.Language.ToDisplayString()}");
            _consoleUI.WriteLine($"4. Опыт программирования на указанном языке: {survey.ExperienceYears}");
            _consoleUI.WriteLine($"5. Мобильный телефон: {survey.PhoneNumber}");
            _consoleUI.WriteLine($"Анкета заполнена: {survey.CreatedAt:dd.MM.yyyy}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _consoleUI.ShowError($"Ошибка при поиске анкеты: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }
}