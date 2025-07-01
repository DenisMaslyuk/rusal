using System.Globalization;
using SurveyApp.Application.Validators;
using SurveyApp.Core.Common;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public sealed class SurveyQuestionService
{
    private readonly FullNameValidationStrategy _nameValidator;
    private readonly DateValidationStrategy _dateValidator;
    private readonly ExperienceValidationStrategy _experienceValidator;
    private readonly PhoneValidationStrategy _phoneValidator;
    private readonly IConsoleUI _consoleUI;

    public SurveyQuestionService(
        FullNameValidationStrategy nameValidator,
        DateValidationStrategy dateValidator,
        ExperienceValidationStrategy experienceValidator,
        PhoneValidationStrategy phoneValidator,
        IConsoleUI consoleUI)
    {
        _nameValidator = nameValidator;
        _dateValidator = dateValidator;
        _experienceValidator = experienceValidator;
        _phoneValidator = phoneValidator;
        _consoleUI = consoleUI;
    }

    public async Task<Result<string>> AskForFullName(string input)
    {
        var validation = _nameValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result<string>.Failure(validation.ErrorMessage);
        }

        return Result<string>.Success(input);
    }

    public async Task<Result<DateTime>> AskForBirthDate(string input)
    {
        var validation = _dateValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result<DateTime>.Failure(validation.ErrorMessage);
        }

        var date = DateTime.ParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        return Result<DateTime>.Success(date);
    }

    public async Task<Result<ProgrammingLanguage>> AskForProgrammingLanguage(string input)
    {
        if (!ProgrammingLanguageExtensions.TryParse(input, out var language))
        {
            var availableLanguages = string.Join(", ", ProgrammingLanguageExtensions.GetAllDisplayNames());
            _consoleUI.ShowError($"Неверный язык программирования. Доступные варианты: {availableLanguages}");
            return Result<ProgrammingLanguage>.Failure("Неверный язык программирования");
        }

        return Result<ProgrammingLanguage>.Success(language);
    }

    public async Task<Result<int>> AskForExperience(string input)
    {
        var validation = _experienceValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result<int>.Failure(validation.ErrorMessage);
        }

        var experience = int.Parse(input);
        return Result<int>.Success(experience);
    }

    public async Task<Result<string>> AskForPhoneNumber(string input)
    {
        var validation = _phoneValidator.Validate(input);
        if (!validation.IsValid)
        {
            _consoleUI.ShowError(validation.ErrorMessage);
            return Result<string>.Failure(validation.ErrorMessage);
        }

        return Result<string>.Success(input);
    }
}