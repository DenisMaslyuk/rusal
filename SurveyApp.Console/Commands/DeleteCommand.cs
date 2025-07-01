using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class DeleteCommand : ICommand
{
    private readonly ISurveyRepository _repository;
    private readonly IConsoleUI _consoleUI;

    public string Name => "-delete";
    public string Description => "Удалить указанную анкету";

    public DeleteCommand(ISurveyRepository repository, IConsoleUI consoleUI)
    {
        _repository = repository;
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            _consoleUI.ShowError("Укажите имя файла анкеты для удаления");
            return Result.Failure("Не указано имя файла");
        }

        var fileName = args[0];
        
        try
        {
            var deleted = await _repository.DeleteAsync(fileName).ConfigureAwait(false);
            
            if (deleted)
            {
                _consoleUI.ShowSuccess($"Анкета '{fileName}' успешно удалена");
                return Result.Success();
            }
            else
            {
                _consoleUI.ShowError($"Анкета с именем файла '{fileName}' не найдена");
                return Result.Failure("Анкета не найдена");
            }
        }
        catch (Exception ex)
        {
            _consoleUI.ShowError($"Ошибка при удалении анкеты: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }
}