using System.Globalization;
using SurveyApp.Application.Commands;
using SurveyApp.Application.Validators;
using SurveyApp.Core.Common;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Commands;

public sealed class NewProfileCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;
    private readonly ApplicationContext _context;
    private readonly FullNameValidationStrategy _nameValidator;
    private readonly DateValidationStrategy _dateValidator;
    private readonly ExperienceValidationStrategy _experienceValidator;
    private readonly PhoneValidationStrategy _phoneValidator;

    public string Name => "-new_profile";
    public string Description => "Заполнить новую анкету";

    public NewProfileCommand(IConsoleUI consoleUI, ApplicationContext context)
    {
        _consoleUI = consoleUI;
        _context = context;
        _nameValidator = new FullNameValidationStrategy();
        _dateValidator = new DateValidationStrategy();
        _experienceValidator = new ExperienceValidationStrategy();
        _phoneValidator = new PhoneValidationStrategy();
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 0;
        _context.SurveyBuilder.Reset();

        await FillSurveyAsync();

        return Result.Success();
    }

    private async Task FillSurveyAsync()
    {
        var questions = new (string, Func<string, Task<Result>>)[]
        {
            ("ФИО", AskForFullName),
            ("Дата рождения (Формат ДД.ММ.ГГГГ)", AskForBirthDate),
            ("Любимый язык программирования", AskForProgrammingLanguage),
            ("Опыт программирования на указанном языке (Полных лет)", AskForExperience),
            ("Мобильный телефон", AskForPhoneNumber)
        };

        while (_context.CurrentQuestionIndex < questions.Length)
        {
            var (questionText, askMethod) = questions[_context.CurrentQuestionIndex];

            _consoleUI.WriteLine($"{_context.CurrentQuestionIndex + 1}. {questionText}:");

            var input = _consoleUI.ReadLine();

            if (IsNavigationCommand(input))
            {
                HandleNavigationCommand(input);
                continue;
            }

            var result = await askMethod(input);
            if (result.IsSuccess)
            {
                _context.CurrentQuestionIndex++;
            }
        }

        _context.CurrentSurvey = _context.SurveyBuilder.Build();
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
            _context.SurveyBuilder.Reset();
        }
        else if (input.StartsWith("-goto_question"))
        {
            var parts = input.Split(' ');
            if (parts.Length > 1 && int.TryParse(parts[1], out var questionNumber))
            {
                if (questionNumber >= 1 && questionNumber <= 5)
                {
                    _context.CurrentQuestionIndex = questionNumber - 1;
                }
            }
        }
    }

    private async Task<Result> AskForFullName(string input)
    {
        var validation = _nameValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result.Failure(validation.ErrorMessage);
        }

        _context.SurveyBuilder.SetFullName(input);
        return Result.Success();
    }

    private async Task<Result> AskForBirthDate(string input)
    {
        var validation = _dateValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result.Failure(validation.ErrorMessage);
        }

        var date = DateTime.ParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        _context.SurveyBuilder.SetBirthDate(date);
        return Result.Success();
    }

    private async Task<Result> AskForProgrammingLanguage(string input)
    {
        if (!ProgrammingLanguageExtensions.TryParse(input, out var language))
        {
            var availableLanguages = string.Join(", ", ProgrammingLanguageExtensions.GetAllDisplayNames());
            _consoleUI.ShowError($"Неверный язык программирования. Доступные варианты: {availableLanguages}");
            return Result.Failure("Неверный язык программирования");
        }

        _context.SurveyBuilder.SetLanguage(language);
        return Result.Success();
    }

    private async Task<Result> AskForExperience(string input)
    {
        var validation = _experienceValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result.Failure(validation.ErrorMessage);
        }

        var experience = int.Parse(input);
        _context.SurveyBuilder.SetExperienceYears(experience);
        return Result.Success();
    }

    private async Task<Result> AskForPhoneNumber(string input)
    {
        var validation = _phoneValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result.Failure(validation.ErrorMessage);
        }

        _context.SurveyBuilder.SetPhoneNumber(input);
        return Result.Success();
    }
}
