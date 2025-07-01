using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Builders;

public sealed class SurveyBuilder
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private string _fullName = string.Empty;
    private DateTime _birthDate;
    private ProgrammingLanguage _language;
    private int _experienceYears;
    private string _phoneNumber = string.Empty;

    public SurveyBuilder(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public void SetFullName(string fullName)
    {
        _fullName = fullName;
    }

    public void SetBirthDate(DateTime birthDate)
    {
        _birthDate = birthDate;
    }

    public void SetLanguage(ProgrammingLanguage language)
    {
        _language = language;
    }

    public void SetExperienceYears(int experienceYears)
    {
        _experienceYears = experienceYears;
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
    }

    public ValidationResult ValidateCompleteness()
    {
        var missing = new List<string>();
        
        if (string.IsNullOrWhiteSpace(_fullName))
            missing.Add("ФИО");
        
        if (_birthDate == default)
            missing.Add("Дата рождения");
        
        if (string.IsNullOrWhiteSpace(_phoneNumber))
            missing.Add("Мобильный телефон");
        
        return missing.Any() 
            ? ValidationResult.Failure($"Не заполнены обязательные поля: {string.Join(", ", missing)}")
            : ValidationResult.Success();
    }

    public (int completed, int total, List<string> answeredFields, List<string> missingFields) GetProgress()
    {
        var answered = new List<string>();
        var missing = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(_fullName))
            answered.Add("ФИО");
        else
            missing.Add("ФИО");
            
        if (_birthDate != default)
            answered.Add("Дата рождения");
        else
            missing.Add("Дата рождения");
            
        answered.Add($"Язык программирования ({_language.ToDisplayString()})");
        
        answered.Add($"Опыт программирования ({_experienceYears} лет)");
        
        if (!string.IsNullOrWhiteSpace(_phoneNumber))
            answered.Add("Мобильный телефон");
        else
            missing.Add("Мобильный телефон");
        
        return (answered.Count, 5, answered, missing);
    }

    public string GetCurrentAnswer(int questionIndex)
    {
        return questionIndex switch
        {
            0 => _fullName,
            1 => _birthDate == default ? string.Empty : _birthDate.ToString("dd.MM.yyyy"),
            2 => _language.ToDisplayString(),
            3 => _experienceYears.ToString(),
            4 => _phoneNumber,
            _ => string.Empty
        };
    }

    public bool HasAnswer(int questionIndex)
    {
        return questionIndex switch
        {
            0 => !string.IsNullOrWhiteSpace(_fullName),
            1 => _birthDate != default,
            2 => true,
            3 => true,
            4 => !string.IsNullOrWhiteSpace(_phoneNumber),
            _ => false
        };
    }

    public Survey Build()
    {
        return new Survey
        {
            FullName = _fullName,
            BirthDate = _birthDate,
            Language = _language,
            ExperienceYears = _experienceYears,
            PhoneNumber = _phoneNumber,
            CreatedAt = _dateTimeProvider.Now
        };
    }

    public void Reset()
    {
        _fullName = string.Empty;
        _birthDate = default;
        _language = default;
        _experienceYears = 0;
        _phoneNumber = string.Empty;
    }
}