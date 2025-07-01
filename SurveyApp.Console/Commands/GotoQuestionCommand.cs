using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class GotoQuestionCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;

    public string Name => "-goto_question";
    public string Description => "Перейти к указанному вопросу (доступно только во время заполнения анкеты)";

    public GotoQuestionCommand(IConsoleUI consoleUI, ApplicationContext context)
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

        if (args.Length < 1)
        {
            _consoleUI.ShowError("Укажите номер вопроса. Пример: -goto_question 3");
            return Result.Failure("Не указан номер вопроса");
        }

        if (!int.TryParse(args[0], out var questionNumber))
        {
            _consoleUI.ShowError("Номер вопроса должен быть числом");
            return Result.Failure("Неверный номер вопроса");
        }

        var surveyBuilder = _context.CurrentSurveyBuilder;
        if (surveyBuilder == null)
        {
            _consoleUI.ShowError("Анкета не инициализирована");
            return Result.Failure("Анкета не инициализирована");
        }

        var totalQuestions = surveyBuilder.GetQuestionCount();
        if (questionNumber < 1 || questionNumber > totalQuestions)
        {
            _consoleUI.ShowError($"Номер вопроса должен быть от 1 до {totalQuestions}");
            return Result.Failure("Номер вопроса вне диапазона");
        }

        var targetIndex = questionNumber - 1;
        var currentIndex = _context.CurrentQuestionIndex;
        
        if (targetIndex > currentIndex)
        {
            var (_, _, _, missingFields) = surveyBuilder.GetProgress();
            if (missingFields.Any())
            {
                _consoleUI.WriteLine($"⚠️  Предупреждение: Вы переходите к вопросу {questionNumber}, пропуская незаполненные поля:");
                foreach (var field in missingFields)
                {
                    _consoleUI.WriteLine($"   • {field}");
                }
                _consoleUI.WriteLine("Продолжить переход? (y/n):");
                var confirmation = _consoleUI.ReadLine();
                if (confirmation?.ToLower() != "y")
                {
                    _consoleUI.WriteLine("Переход отменен");
                    return Result.Success();
                }
            }
        }

        _context.CurrentQuestionIndex = targetIndex;
        _consoleUI.ShowSuccess($"Переход к вопросу {questionNumber}");
        return Result.Success();
    }
}