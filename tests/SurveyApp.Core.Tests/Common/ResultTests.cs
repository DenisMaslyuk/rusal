using FluentAssertions;
using SurveyApp.Core.Common;

namespace SurveyApp.Core.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        ((bool)result).Should().BeTrue();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(errorMessage);
        ((bool)result).Should().BeFalse();
    }

    [Fact]
    public void Failure_WithNullError_ShouldAcceptNull()
    {
        // Act
        var result = Result.Failure(null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeNull();
    }
}

public class ResultGenericTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        const string value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
        ((bool)result).Should().BeTrue();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithoutValue()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(errorMessage);
        ((bool)result).Should().BeFalse();
    }

    [Fact]
    public void Success_WithNullValue_ShouldAcceptNull()
    {
        // Act
        var result = Result<string>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithGenericType_ShouldHaveDefaultValue()
    {
        // Act
        var result = Result<int>.Failure("error");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(0); // default(int)
        result.Error.Should().Be("error");
    }
}