using SurveyApp.Core.Enums;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Builders;

public sealed class SurveyBuilder
{
    private string _fullName = string.Empty;
    private DateTime _birthDate;
    private ProgrammingLanguage _language;
    private int _experienceYears;
    private string _phoneNumber = string.Empty;

    public SurveyBuilder SetFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public SurveyBuilder SetBirthDate(DateTime birthDate)
    {
        _birthDate = birthDate;
        return this;
    }

    public SurveyBuilder SetLanguage(ProgrammingLanguage language)
    {
        _language = language;
        return this;
    }

    public SurveyBuilder SetExperienceYears(int experienceYears)
    {
        _experienceYears = experienceYears;
        return this;
    }

    public SurveyBuilder SetPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
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
            CreatedAt = DateTime.Now
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