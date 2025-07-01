using FluentAssertions;
using SurveyApp.Application.Processors;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Processors;

public class TextQuestionProcessorTests
{
    private readonly TextQuestionProcessor _processor = new();

    [Fact]
    public void SupportedType_ShouldReturnText()
    {
        _processor.SupportedType.Should().Be(QuestionType.Text);
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
    public void ValidateAnswer_RequiredAndWhitespace_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("   ", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("не может быть пустым");
    }

    [Fact]
    public void ValidateAnswer_NotRequiredAndEmpty_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = false };

        // Act
        var result = _processor.ValidateAnswer("", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateAnswer_ValidText_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("Valid text", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateAnswer_WithMinLength_TooShort_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> { ["MinLength"] = 5 }
        };

        // Act
        var result = _processor.ValidateAnswer("Hi", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Минимальная длина: 5");
    }

    [Fact]
    public void ValidateAnswer_WithMaxLength_TooLong_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> { ["MaxLength"] = 10 }
        };

        // Act
        var result = _processor.ValidateAnswer("This text is way too long", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Максимальная длина: 10");
    }

    [Fact]
    public void FormatAnswer_ShouldTrimWhitespace()
    {
        // Arrange
        var question = new QuestionDefinition();

        // Act
        var result = _processor.FormatAnswer("  Test Answer  ", question);

        // Assert
        result.Should().Be("Test Answer");
    }

    [Fact]
    public void GetPrompt_WithoutOptions_ShouldReturnBasicPrompt()
    {
        // Arrange
        var question = new QuestionDefinition { Prompt = "Enter your name" };

        // Act
        var result = _processor.GetPrompt(question);

        // Assert
        result.Should().Be("Enter your name:");
    }

    [Fact]
    public void GetPrompt_WithLengthOptions_ShouldIncludeConstraints()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            Prompt = "Enter your name",
            Options = new Dictionary<string, object> 
            { 
                ["MinLength"] = 2,
                ["MaxLength"] = 50
            }
        };

        // Act
        var result = _processor.GetPrompt(question);

        // Assert
        result.Should().Be("Enter your name (мин. 2 символов, макс. 50 символов):");
    }
}