using FluentAssertions;
using Microsoft.Extensions.Options;
using SurveyApp.Application.Tests.TestHelpers;
using SurveyApp.Application.Validators;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Validators;

public class DateValidationStrategyTests
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SurveySettings _settings;
    private readonly DateValidationStrategy _validator;

    public DateValidationStrategyTests()
    {
        _dateTimeProvider = new TestDateTimeProvider
        {
            Today = new DateTime(2024, 6, 15)
        };
        _settings = new SurveySettings
        {
            DateFormat = "dd.MM.yyyy",
            MinAge = 0,
            MaxAge = 120
        };
        var options = Options.Create(_settings);
        _validator = new DateValidationStrategy(_dateTimeProvider, options);
    }

    [Fact]
    public void Validate_NullOrEmptyDate_ShouldReturnFailure()
    {
        // Act & Assert
        _validator.Validate(null!).IsValid.Should().BeFalse();
        _validator.Validate("").IsValid.Should().BeFalse();
        _validator.Validate("   ").IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidFormat_ShouldReturnFailure()
    {
        // Arrange
        var invalidDates = new[] { "15/06/1990", "1990-06-15", "15.6.1990", "invalid" };

        // Act & Assert
        foreach (var invalidDate in invalidDates)
        {
            var result = _validator.Validate(invalidDate);
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain(_settings.DateFormat);
        }
    }

    [Fact]
    public void Validate_ValidDate_ShouldReturnSuccess()
    {
        // Arrange - 30 years old (valid age)
        var validDate = "15.06.1994";

        // Act
        var result = _validator.Validate(validDate);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void Validate_FutureDate_ShouldReturnFailure()
    {
        // Arrange - Дата в будущем
        var futureDate = "15.06.2025";

        // Act
        var result = _validator.Validate(futureDate);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("будущем");
    }

    [Fact]
    public void Validate_ExactlyMinAge_ShouldReturnSuccess()
    {
        // Arrange - Exactly 0 years old (born today)
        var exactMinAgeDate = "15.06.2024";

        // Act
        var result = _validator.Validate(exactMinAgeDate);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TooOld_ShouldReturnFailure()
    {
        // Arrange - 121 years old (too old)
        var tooOldDate = "15.06.1903";

        // Act
        var result = _validator.Validate(tooOldDate);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain($"{_settings.MaxAge} лет");
    }

    [Fact]
    public void Validate_ExactlyMaxAge_ShouldReturnSuccess()
    {
        // Arrange - Exactly 120 years old
        var exactMaxAgeDate = "15.06.1904";

        // Act
        var result = _validator.Validate(exactMaxAgeDate);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_BorderCases_ShouldHandleCorrectly()
    {
        // Arrange
        var dayBeforeToday = "14.06.2024"; // Day before today (valid)
        var tomorrow = "16.06.2024";  // Day after today (future - invalid)

        // Act
        var resultBefore = _validator.Validate(dayBeforeToday);
        var resultTomorrow = _validator.Validate(tomorrow);

        // Assert
        resultBefore.IsValid.Should().BeTrue();   // Valid past date
        resultTomorrow.IsValid.Should().BeFalse(); // Future date invalid
    }

    [Theory]
    [InlineData("29.02.2000")] // Valid leap year
    [InlineData("28.02.2001")] // Valid non-leap year
    public void Validate_ValidSpecialDates_ShouldReturnSuccess(string date)
    {
        // Act
        var result = _validator.Validate(date);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("29.02.2001")] // Invalid leap year
    [InlineData("31.02.2024")] // Invalid day for February
    [InlineData("31.04.2024")] // Invalid day for April
    public void Validate_InvalidDates_ShouldReturnFailure(string date)
    {
        // Act
        var result = _validator.Validate(date);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    // Тесты для конфигурируемых возрастных ограничений
    [Fact]
    public void Validate_WithCustomMinAge_ShouldRespectSetting()
    {
        // Arrange - создаем валидатор с минимальным возрастом 18
        var customSettings = new SurveySettings
        {
            DateFormat = "dd.MM.yyyy",
            MinAge = 18,
            MaxAge = 120
        };
        var customOptions = Options.Create(customSettings);
        var customValidator = new DateValidationStrategy(_dateTimeProvider, customOptions);
        
        var age17Date = "15.06.2007"; // 17 лет - должно быть невалидно
        var age18Date = "15.06.2006"; // 18 лет - должно быть валидно

        // Act
        var result17 = customValidator.Validate(age17Date);
        var result18 = customValidator.Validate(age18Date);

        // Assert
        result17.IsValid.Should().BeFalse();
        result17.ErrorMessage.Should().Contain("18 лет");
        result18.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithCustomMaxAge_ShouldRespectSetting()
    {
        // Arrange - создаем валидатор с максимальным возрастом 100
        var customSettings = new SurveySettings
        {
            DateFormat = "dd.MM.yyyy",
            MinAge = 0,
            MaxAge = 100
        };
        var customOptions = Options.Create(customSettings);
        var customValidator = new DateValidationStrategy(_dateTimeProvider, customOptions);
        
        var age100Date = "15.06.1924"; // 100 лет - должно быть валидно
        var age101Date = "15.06.1923"; // 101 год - должно быть невалидно

        // Act
        var result100 = customValidator.Validate(age100Date);
        var result101 = customValidator.Validate(age101Date);

        // Assert
        result100.IsValid.Should().BeTrue();
        result101.IsValid.Should().BeFalse();
        result101.ErrorMessage.Should().Contain("100 лет");
    }

    [Fact]
    public void Validate_WithMinAgeZero_ShouldAllowNewborns()
    {
        // Arrange - новорожденный (сегодняшняя дата)
        var newbornDate = "15.06.2024";

        // Act
        var result = _validator.Validate(newbornDate);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(16, 65)]
    [InlineData(21, 120)]
    public void Validate_WithDifferentAgeRanges_ShouldWorkCorrectly(int minAge, int maxAge)
    {
        // Arrange
        var customSettings = new SurveySettings
        {
            DateFormat = "dd.MM.yyyy",
            MinAge = minAge,
            MaxAge = maxAge
        };
        var customOptions = Options.Create(customSettings);
        var customValidator = new DateValidationStrategy(_dateTimeProvider, customOptions);
        
        var validAgeDate = _dateTimeProvider.Today.AddYears(-(minAge + maxAge) / 2).ToString("dd.MM.yyyy");
        var tooYoungDate = _dateTimeProvider.Today.AddYears(-minAge).AddDays(1).ToString("dd.MM.yyyy");
        var tooOldDate = _dateTimeProvider.Today.AddYears(-maxAge).AddDays(-1).ToString("dd.MM.yyyy");

        // Act
        var validResult = customValidator.Validate(validAgeDate);
        var youngResult = customValidator.Validate(tooYoungDate);
        var oldResult = customValidator.Validate(tooOldDate);

        // Assert
        validResult.IsValid.Should().BeTrue();
        
        if (minAge > 0)
        {
            youngResult.IsValid.Should().BeFalse();
            youngResult.ErrorMessage.Should().Contain($"{minAge} лет");
        }
        
        oldResult.IsValid.Should().BeFalse();
        oldResult.ErrorMessage.Should().Contain($"{maxAge} лет");
    }
}