using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class RestartProfileCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;

    public string Name => "-restart_profile";
    public string Description => "Начать заполнение анкеты заново (доступно только во время заполнения анкеты)";

    public RestartProfileCommand(IConsoleUI consoleUI, ApplicationContext context)
    {
        _consoleUI = consoleUI;
        _context = context;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        if (!_context.IsInSurveyMode)
        {
            _consoleUI.ShowError("Команда доступна только во время заполнения анкеты");
            return Result.Failure("Не в режиме анкетирования");
        }

        _context.CurrentQuestionIndex = 0;
        _consoleUI.ShowSuccess("Анкета сброшена. Начинаем заново с первого вопроса");
        return Result.Success();
    }
}