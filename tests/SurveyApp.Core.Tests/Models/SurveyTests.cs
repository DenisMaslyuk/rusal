using FluentAssertions;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Models;

namespace SurveyApp.Core.Tests.Models;

public class SurveyTests
{
    [Fact]
    public void CalculateAge_WithValidBirthDate_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 5, 15);
        var referenceDate = new DateTime(2024, 5, 15); // Exactly 34 years
        var survey = new Survey
        {
            BirthDate = birthDate,
            FullName = "Test User",
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        // Act
        var age = survey.CalculateAge(referenceDate);

        // Assert
        age.Should().Be(34);
    }

    [Fact]
    public void CalculateAge_BeforeBirthday_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 5, 15);
        var referenceDate = new DateTime(2024, 5, 14); // Day before birthday
        var survey = new Survey
        {
            BirthDate = birthDate,
            FullName = "Test User",
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        // Act
        var age = survey.CalculateAge(referenceDate);

        // Assert
        age.Should().Be(33); // Should be 33, not 34
    }

    [Fact]
    public void CalculateAge_AfterBirthday_ShouldReturnCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 5, 15);
        var referenceDate = new DateTime(2024, 5, 16); // Day after birthday
        var survey = new Survey
        {
            BirthDate = birthDate,
            FullName = "Test User",
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        // Act
        var age = survey.CalculateAge(referenceDate);

        // Assert
        age.Should().Be(34);
    }

    [Fact]
    public void CalculateAge_LeapYear_ShouldHandleCorrectly()
    {
        // Arrange
        var birthDate = new DateTime(2000, 2, 29); // Leap year birthday
        var referenceDate = new DateTime(2024, 2, 28); // Day before leap year birthday
        var survey = new Survey
        {
            BirthDate = birthDate,
            FullName = "Test User",
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        // Act
        var age = survey.CalculateAge(referenceDate);

        // Assert
        age.Should().Be(23); // Should be 23, not 24
    }

    [Fact]
    public void Survey_ShouldBeImmutableRecord()
    {
        // Arrange
        var survey1 = new Survey
        {
            FullName = "John Doe",
            BirthDate = new DateTime(1990, 1, 1),
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        var survey2 = new Survey
        {
            FullName = "John Doe",
            BirthDate = new DateTime(1990, 1, 1),
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = survey1.CreatedAt
        };

        // Act & Assert
        survey1.Should().Be(survey2); // Records should be equal if all properties are equal
        survey1.GetHashCode().Should().Be(survey2.GetHashCode());
    }
}