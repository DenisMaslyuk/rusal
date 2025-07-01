using SurveyApp.Application.Commands;
using SurveyApp.Application.Services;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class NewProfileCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;
    private readonly SurveyBuilderFactory _surveyBuilderFactory;
    private ISurveyBuilder? _surveyBuilder;

    public string Name => "-new_profile";
    public string Description => "Заполнить новую анкету";

    public NewProfileCommand(
        IConsoleUI consoleUI, 
        ApplicationContext context,
        SurveyBuilderFactory surveyBuilderFactory)
    {
        _consoleUI = consoleUI;
        _context = context;
        _surveyBuilderFactory = surveyBuilderFactory;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        _surveyBuilder = _surveyBuilderFactory.CreateSurveyBuilder("developer");
        _context.CurrentSurveyBuilder = _surveyBuilder;
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 0;

        await FillSurveyAsync().ConfigureAwait(false);

        return Result.Success();
    }

    private async Task FillSurveyAsync()
    {
        var totalQuestions = _surveyBuilder!.GetQuestionCount();

        while (_context.CurrentQuestionIndex < totalQuestions)
        {
            ShowProgress();
            
            var prompt = _surveyBuilder.GetQuestionPrompt(_context.CurrentQuestionIndex);
            _consoleUI.WriteLine($"{_context.CurrentQuestionIndex + 1}. {prompt}");
            
            ShowCurrentAnswer(_context.CurrentQuestionIndex);

            var input = _consoleUI.ReadLine();

            if (IsNavigationCommand(input))
            {
                HandleNavigationCommand(input);
                continue;
            }

            if (string.IsNullOrWhiteSpace(input) && _surveyBuilder.HasAnswer(_context.CurrentQuestionIndex))
            {
                _consoleUI.WriteLine("✅ Сохранен текущий ответ");
                _context.CurrentQuestionIndex++;
            }
            else
            {
                var result = _surveyBuilder.SetAnswer(_context.CurrentQuestionIndex, input);
                if (result.IsValid)
                {
                    _consoleUI.WriteLine("✅ Ответ сохранен");
                    _context.CurrentQuestionIndex++;
                }
                else
                {
                    _consoleUI.ShowError(result.ErrorMessage);
                }
            }
        }

        var validationResult = _surveyBuilder.ValidateCompleteness();
        if (!validationResult.IsValid)
        {
            _consoleUI.ShowError(validationResult.ErrorMessage);
            _consoleUI.WriteLine("Используйте команды навигации для возврата к пропущенным вопросам:");
            _consoleUI.WriteLine("  -goto_question <номер> - перейти к вопросу");
            _consoleUI.WriteLine("  -goto_prev_question - к предыдущему вопросу");
            _consoleUI.WriteLine("  -restart_profile - начать заново");
            
            // Остаемся в режиме заполнения анкеты, позволяя навигацию
            while (_context.IsInSurveyMode)
            {
                _consoleUI.WriteLine("");
                _consoleUI.Write("Введите команду навигации: ");
                var navInput = _consoleUI.ReadLine().Trim();
                
                if (string.IsNullOrWhiteSpace(navInput))
                    continue;
                    
                if (IsNavigationCommand(navInput))
                {
                    HandleNavigationCommand(navInput);
                    if (!_context.IsInSurveyMode) break; // Если пользователь перезапустил
                    
                    // Возвращаемся к заполнению с текущего вопроса
                    await ContinueSurveyFromCurrentQuestion();
                    
                    // Проверяем, заполнены ли все поля после продолжения
                    var newValidationResult = _surveyBuilder.ValidateCompleteness();
                    if (newValidationResult.IsValid)
                    {
                        // Все поля заполнены, выходим из навигации
                        break;
                    }
                    else
                    {
                        _consoleUI.ShowError(newValidationResult.ErrorMessage);
                        continue;
                    }
                }
                else
                {
                    _consoleUI.ShowError("Доступны только команды навигации. Используйте -goto_question <номер>, -goto_prev_question или -restart_profile");
                }
            }
            
            // Если все поля заполнены, продолжаем с сохранением
            if (_context.IsInSurveyMode)
            {
                var finalValidation = _surveyBuilder.ValidateCompleteness();
                if (!finalValidation.IsValid)
                {
                    _consoleUI.ShowError("Анкета все еще не заполнена полностью.");
                    return;
                }
            }
            else
            {
                return; // Пользователь перезапустил или вышел
            }
        }

        _context.CurrentSurvey = _surveyBuilder.BuildSurvey();
        _context.IsInSurveyMode = false;

        _consoleUI.ShowSuccess("Анкета заполнена успешно!");
        _consoleUI.WriteLine("Выберите действие:");
    }

    private bool IsNavigationCommand(string input)
    {
        return input.StartsWith("-goto_question") ||
               input == "-goto_prev_question" ||
               input == "-restart_profile";
    }

    private void HandleNavigationCommand(string input)
    {
        var totalQuestions = _surveyBuilder!.GetQuestionCount();
        
        if (input == "-goto_prev_question")
        {
            if (_context.CurrentQuestionIndex > 0)
            {
                _context.CurrentQuestionIndex--;
            }
        }
        else if (input == "-restart_profile")
        {
            _context.CurrentQuestionIndex = 0;
            _surveyBuilder = _surveyBuilderFactory.CreateSurveyBuilder("developer");
            _context.CurrentSurveyBuilder = _surveyBuilder;
        }
        else if (input.StartsWith("-goto_question"))
        {
            var parts = input.Split(' ');
            if (parts.Length > 1 && int.TryParse(parts[1], out var questionNumber))
            {
                if (questionNumber >= 1 && questionNumber <= totalQuestions)
                {
                    _context.CurrentQuestionIndex = questionNumber - 1;
                }
            }
        }
    }

    private void ShowProgress()
    {
        var (completed, total, answeredFields, missingFields) = _surveyBuilder.GetProgress();
        
        _consoleUI.WriteLine($"\n📊 Прогресс: {completed}/{total} полей заполнено");
        
        if (answeredFields.Any())
        {
            _consoleUI.WriteLine("✅ Заполнено:");
            foreach (var field in answeredFields)
            {
                _consoleUI.WriteLine($"   • {field}");
            }
        }
        
        if (missingFields.Any())
        {
            _consoleUI.WriteLine("⚠️  Требует заполнения:");
            foreach (var field in missingFields)
            {
                _consoleUI.WriteLine($"   • {field}");
            }
        }
        
        _consoleUI.WriteLine("");
    }

    private void ShowCurrentAnswer(int questionIndex)
    {
        if (_surveyBuilder.HasAnswer(questionIndex))
        {
            var currentAnswer = _surveyBuilder.GetCurrentAnswer(questionIndex);
            _consoleUI.WriteLine($"💡 Текущий ответ: {currentAnswer}");
            _consoleUI.WriteLine("Нажмите Enter для сохранения или введите новый ответ:");
        }
    }

    private async Task ContinueSurveyFromCurrentQuestion()
    {
        var totalQuestions = _surveyBuilder!.GetQuestionCount();

        while (_context.CurrentQuestionIndex < totalQuestions)
        {
            ShowProgress();
            
            var prompt = _surveyBuilder.GetQuestionPrompt(_context.CurrentQuestionIndex);
            _consoleUI.WriteLine($"{_context.CurrentQuestionIndex + 1}. {prompt}");
            
            ShowCurrentAnswer(_context.CurrentQuestionIndex);

            var input = _consoleUI.ReadLine();

            if (IsNavigationCommand(input))
            {
                HandleNavigationCommand(input);
                continue;
            }

            if (string.IsNullOrWhiteSpace(input) && _surveyBuilder.HasAnswer(_context.CurrentQuestionIndex))
            {
                _consoleUI.WriteLine("✅ Сохранен текущий ответ");
                _context.CurrentQuestionIndex++;
            }
            else
            {
                var result = _surveyBuilder.SetAnswer(_context.CurrentQuestionIndex, input);
                if (result.IsValid)
                {
                    _consoleUI.WriteLine("✅ Ответ сохранен");
                    _context.CurrentQuestionIndex++;
                }
                else
                {
                    _consoleUI.ShowError(result.ErrorMessage);
                }
            }
        }
    }
}
