using FluentAssertions;
using SurveyApp.Application.Validators;

namespace SurveyApp.Application.Tests.Validators;

public class FullNameValidationStrategyTests
{
    private readonly FullNameValidationStrategy _validator = new();

    [Fact]
    public void Validate_NullOrEmptyName_ShouldReturnFailure()
    {
        // Act & Assert
        _validator.Validate(null!).IsValid.Should().BeFalse();
        _validator.Validate("").IsValid.Should().BeFalse();
        _validator.Validate("   ").IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("Иван Петров")]
    [InlineData("Мария Ивановна Сидорова")]
    [InlineData("John Doe")]
    [InlineData("Jean-Claude Van Damme")]
    [InlineData("Mary O'Connor")]
    public void Validate_ValidNames_ShouldReturnSuccess(string name)
    {
        // Act
        var result = _validator.Validate(name);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("А")]
    [InlineData("Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Very Long Name")]
    public void Validate_InvalidNames_ShouldReturnFailure(string name)
    {
        // Act
        var result = _validator.Validate(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ExactlyMinLength_ShouldReturnSuccess()
    {
        // Arrange - Exactly 2 characters
        var name = "Ая";

        // Act
        var result = _validator.Validate(name);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ExactlyMaxLength_ShouldReturnSuccess()
    {
        // Arrange - Exactly 100 characters
        var name = new string('А', 100);

        // Act
        var result = _validator.Validate(name);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Test123")]
    [InlineData("Test@Name")]
    [InlineData("Test#Name")]
    [InlineData("Test$Name")]
    public void Validate_NameWithInvalidCharacters_ShouldReturnFailure(string name)
    {
        // Act
        var result = _validator.Validate(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("буквы");
    }
}