using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class GotoPrevQuestionCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;

    public string Name => "-goto_prev_question";
    public string Description => "Перейти к предыдущему вопросу (доступно только во время заполнения анкеты)";

    public GotoPrevQuestionCommand(IConsoleUI consoleUI, ApplicationContext context)
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

        if (_context.CurrentQuestionIndex <= 0)
        {
            _consoleUI.ShowError("Вы уже находитесь на первом вопросе");
            return Result.Failure("Уже на первом вопросе");
        }

        _context.CurrentQuestionIndex--;
        _consoleUI.ShowSuccess($"Переход к вопросу {_context.CurrentQuestionIndex + 1}");
        return Result.Success();
    }
}