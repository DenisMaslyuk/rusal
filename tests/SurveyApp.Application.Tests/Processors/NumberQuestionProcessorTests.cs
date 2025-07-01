using FluentAssertions;
using SurveyApp.Application.Processors;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Processors;

public class NumberQuestionProcessorTests
{
    private readonly NumberQuestionProcessor _processor = new();

    [Fact]
    public void SupportedType_ShouldReturnNumber()
    {
        _processor.SupportedType.Should().Be(QuestionType.Number);
    }

    [Fact]
    public void ValidateAnswer_RequiredAndEmpty_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("не может быть пустым");
    }

    [Fact]
    public void ValidateAnswer_ValidInteger_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("42", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateAnswer_ValidDecimal_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("42.5", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateAnswer_InvalidNumber_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("not a number", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("корректное число");
    }

    [Fact]
    public void ValidateAnswer_WithMinConstraint_BelowMin_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> { ["Min"] = 10.0 }
        };

        // Act
        var result = _processor.ValidateAnswer("5", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Минимальное значение: 10");
    }

    [Fact]
    public void ValidateAnswer_WithMaxConstraint_AboveMax_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> { ["Max"] = 100.0 }
        };

        // Act
        var result = _processor.ValidateAnswer("150", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Максимальное значение: 100");
    }

    [Fact]
    public void ValidateAnswer_WithinRange_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> 
            { 
                ["Min"] = 10.0,
                ["Max"] = 100.0
            }
        };

        // Act
        var result = _processor.ValidateAnswer("50", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FormatAnswer_ShouldTrimWhitespace()
    {
        // Arrange
        var question = new QuestionDefinition();

        // Act
        var result = _processor.FormatAnswer("  42  ", question);

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void GetPrompt_WithoutOptions_ShouldReturnBasicPrompt()
    {
        // Arrange
        var question = new QuestionDefinition { Prompt = "Enter number" };

        // Act
        var result = _processor.GetPrompt(question);

        // Assert
        result.Should().Be("Enter number:");
    }

    [Fact]
    public void GetPrompt_WithRangeOptions_ShouldIncludeConstraints()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            Prompt = "Enter age",
            Options = new Dictionary<string, object> 
            { 
                ["Min"] = 0.0,
                ["Max"] = 120.0
            }
        };

        // Act
        var result = _processor.GetPrompt(question);

        // Assert
        result.Should().Be("Enter age (мин. 0, макс. 120):");
    }
}