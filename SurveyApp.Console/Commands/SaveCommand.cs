using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class SaveCommand : ICommand
{
    private readonly ISurveyRepository _repository;
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;

    public string Name => "-save";
    public string Description => "Сохранить заполненную анкету";

    public SaveCommand(ISurveyRepository repository, IConsoleUI consoleUI, ApplicationContext context)
    {
        _repository = repository;
        _consoleUI = consoleUI;
        _context = context;
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
            await _repository.SaveAsync(_context.CurrentSurvey);
            _consoleUI.ShowSuccess($"Анкета успешно сохранена в файл: {_context.CurrentSurvey.GetFileName()}");
            _context.CurrentSurvey = null;
            return Result.Success();
        }
        catch (Exception ex)
        {
            _consoleUI.ShowError($"Ошибка при сохранении анкеты: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }
}