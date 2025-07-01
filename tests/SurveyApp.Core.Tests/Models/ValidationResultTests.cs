using FluentAssertions;
using SurveyApp.Core.Common;
using SurveyApp.Core.Models;

namespace SurveyApp.Core.Tests.Models;

public class ValidationResultTests
{
    [Fact]
    public void Success_ShouldCreateValidResult()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        ((bool)result).Should().BeTrue();
    }

    [Fact]
    public void Failure_ShouldCreateInvalidResult()
    {
        // Arrange
        const string errorMessage = "Validation failed";

        // Act
        var result = ValidationResult.Failure(errorMessage);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        ((bool)result).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversionToResult_Success_ShouldConvertCorrectly()
    {
        // Arrange
        var validationResult = ValidationResult.Success();

        // Act
        Result result = validationResult;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ImplicitConversionToResult_Failure_ShouldConvertCorrectly()
    {
        // Arrange
        const string errorMessage = "Validation failed";
        var validationResult = ValidationResult.Failure(errorMessage);

        // Act
        Result result = validationResult;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void ImplicitConversionToBool_Success_ShouldReturnTrue()
    {
        // Arrange
        var result = ValidationResult.Success();

        // Act & Assert
        ((bool)result).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversionToBool_Failure_ShouldReturnFalse()
    {
        // Arrange
        var result = ValidationResult.Failure("error");

        // Act & Assert
        ((bool)result).Should().BeFalse();
    }
}